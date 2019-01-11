using FluentAssertions;
using TimeTracker.Api.Models;
using Xunit;

namespace TimeTracker.Api.Test
{
    public class InvoiceDtoTest
    {
        [Fact]
        public void FormattedTotal_ReturnsCorrectValues()
        {
            InvoiceDto testSubject = new InvoiceDto();
            
            testSubject.LineItems.Add(
                new LineItem()
                {
                    PricePerItem = 3.6m,
                    Quantity = 3
                });


            testSubject.FormattedTotal.Should().Be("$10.80");
        }
    }
}