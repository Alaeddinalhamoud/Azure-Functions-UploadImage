using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Shared.Models.Medias;
using ShopV2.Web.Data;
using ShopV2.Web.Models;

namespace ShopV2.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                        .AsNoTracking()
                        .Include(x => x.ImageUrl)
                        .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModelView productVM)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = productVM.Name,
                    Price = productVM.Price
                };

                _context.Add(product);
                await _context.SaveChangesAsync();

                // TODO: Image handling
                if (productVM.Files is not null)
                {
                    if (productVM.IsUploadedToAzure)
                    {
                        List<MediaRequest> mediaRequests = new();

                        foreach (var file in productVM.Files)
                        {
                            if (file.Length > 0)
                            {
                                var mediaRequest = new MediaRequest
                                {
                                    ProductId = product.Id,
                                    ContentType = file.ContentType
                                };

                                using (MemoryStream ms = new MemoryStream())
                                {
                                    file.CopyTo(ms);
                                    mediaRequest.Content = Convert.ToBase64String(ms.ToArray());
                                    mediaRequests.Add(mediaRequest);
                                }
                            }
                        }
                        
                        if(!mediaRequests.Any()) return View(productVM);
                        //Send/Post to Az function app ("Create Media")
                        using var httpClient = new HttpClient();
                        var mediaResponse = await httpClient.PostAsJsonAsync("http://localhost:7071/api/Create", mediaRequests);
                        mediaResponse.EnsureSuccessStatusCode();
                    }
                    else
                    {
                        await UploadImageToFolder(productVM, product);
                    }                    
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productVM);
        }

        private async Task UploadImageToFolder(ProductModelView productVM, Product product)
        {
            List<Image> images = new();

            foreach (var file in productVM.Files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine("wwwroot", "images", product.Name + file.FileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                    }
                    images.Add(new Image { ProductId = product.Id, Url = filePath.Replace("wwwroot", "") });
                }
            }

            if (images.Any())
            {
                _context.AddRange(images);
                await _context.SaveChangesAsync();
            }
        }

        // GET: Products/Edit/5
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

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price")] Product product)
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

        // GET: Products/Delete/5
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

        // POST: Products/Delete/5
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
    }
}
