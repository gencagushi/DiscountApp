﻿namespace DiscountServer.Models;

public class GenerateResponse
{
    public bool Result { get; set; }
    public List<string> Codes { get; set; } = [];
}
