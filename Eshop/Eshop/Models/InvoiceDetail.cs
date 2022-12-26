using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Eshop.Models
{
    public class InvoiceDetail
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }

        // Navigation reference property cho khóa ngoại đến Invoice
        [DisplayName("Hóa đơn")]
        public Invoice Invoice { get; set; }

        public int ProductId { get; set; }

        // Navigation reference property cho khóa ngoại đến Product
        [DisplayName("Sản phẩm")]
        public Product Product { get; set; }

        [DisplayName("Số lượng")]
        [DefaultValue(1)]
        public int Quantity { get; set; } = 1;

        [DisplayName("Đơn giá")]
        [DefaultValue(0)]
        public int UnitPrice { get; set; } = 0;
    }
}
