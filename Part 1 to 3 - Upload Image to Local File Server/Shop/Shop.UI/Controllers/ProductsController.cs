using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.UI.Data;
using Shop.UI.Models;

namespace Shop.UI.Controllers;

public class ProductsController(ApplicationDbContext _context) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await _context.Products.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(x => x.ImageUrl)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductModelView productVM)
    {
        if (ModelState.IsValid)
        {
            var product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price,
                Description = productVM.Description
            };

            _context.Add(product);
            await _context.SaveChangesAsync();

            await UploadImageToFolder(productVM, product);

            return RedirectToAction(nameof(Index));
        }
        return View(productVM);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Description")] Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }

    #region Private Methods
    //Uplload Image to local folder (wwwroot/Images)
    async Task UploadImageToFolder(ProductModelView productVM, Product product)
    {
        if (productVM.Files is not null)
        {
            List<ImageUrl> imageUrls = new();
            foreach (var file in productVM.Files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine("wwwroot", "images", productVM.Name + file.FileName);
                    //Moce file to folder
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                        var bytes = System.IO.File.ReadAllBytes(filePath);

                    }
                    imageUrls.Add(new ImageUrl { ProductId = product.Id, Url = filePath.Replace("wwwroot", "") });
                }
            }

            if (imageUrls.Any())
            {
                _context.AddRange(imageUrls);
                await _context.SaveChangesAsync();
            }
        }
    }
    #endregion
}
