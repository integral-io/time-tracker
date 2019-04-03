using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeTracker.Api.Models
{
    public class InvoiceDto
    {
        public InvoiceDto()
        {
            LineItems = new List<LineItem>();
        }

        public string InvoiceNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime DueDate { get; set; }

        public CompanyInfoDto CompanySeller { get; set; }
        public CompanyInfoDto CompanyBuyer { get; set; }
        public ICollection<LineItem> LineItems { get; set; }

        public string FormattedTotal
        {
            get
            {
                var total = LineItems.Sum(x => x.Quantity * x.PricePerItem);
                return $"${total:#.00}";
            }
        }
    }

    public class CompanyInfoDto
    {
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityStateZip { get; set; }
        public string Email { get; set; }
    }

    public class LineItem
    {
        public string Name { get; set; }
        public decimal PricePerItem { get; set; }
        public int Quantity { get; set; }
        public string FormattedTotal
        {
            get
            {
                var total = PricePerItem * Quantity;
                return $"${total:#.00}";
            }
        }
    }
    
    
}