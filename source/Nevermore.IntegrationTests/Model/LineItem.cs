using System;

namespace Nevermore.IntegrationTests.Model
{
    public class LineItem
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}