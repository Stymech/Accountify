namespace Accountify
{
    public class ArticleInfo
    {
        public int ArticleID { get; set; }
        public string ArticleName { get; set; }
        public decimal ArticlePrice { get; set; }
        public string ArticleDescription { get; set; }
    }

    public class ReceiptInfo
    {
        public int ReceiptID { get; set; }
        public int BuyerID { get; set; }
        public string BuyerName { get; set; }
        public string BuyerAddress { get; set; }
        public string ReceiptDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CompanyInfo { get; set; }
        public List<ArticleInfo> ReceiptArticles { get; set; } // List of articles included in the receipt
    }

    public  class ReceiptItems
    {
        public int ReceiptItemID { get; set; }
        public int ReceiptID { get; set; }
        public int ArticleID { get; set; }
        public int Quantity { get; set; }

    }

    public class BuyerInfo
    {
        public int BuyerID { get; set; }
        public string BuyerName { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerTel { get; set; }
        public string BuyerEmail { get; set; }
    }
}
