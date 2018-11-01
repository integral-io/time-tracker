using System;

namespace TimeTracker.Api.Models
{
    public class InvoiceDto
    {
        public String InvoiceNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime DueDate { get; set; }
        
        
    }
    
    
}