﻿namespace ShopV2.Web.Models;

public class ProductModelView : Product
{
    public List<IFormFile>? Files { get; set; }

    public bool IsUploadedToAzure { get; set; }
}
