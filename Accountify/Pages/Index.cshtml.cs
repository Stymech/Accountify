using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Accountify.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<ReceiptInfo> listReceipts = new List<ReceiptInfo>();

        [BindProperty(SupportsGet = true)]
        public string BuyerIdFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? FromDateFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? ToDateFilter { get; set; }
        public string SortColumn { get; set; }
        String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=accountify;Integrated Security=True";

        //////////////////////////////////// methods:

        private List<ReceiptInfo> SortList(List<ReceiptInfo> list, string column)
        {
            switch (column)
            {
                case "ReceiptID":
                    return list.OrderBy(item => item.ReceiptID).ToList();
                case "BuyerID":
                    return list.OrderBy(item => item.BuyerID).ToList();
                case "BuyerName":
                    return list.OrderBy(item => item.BuyerName).ToList();
                case "BuyerAddress":
                    return list.OrderBy(item => item.BuyerAddress).ToList();
                case "ReceiptDate":
                    return list.OrderBy(item => item.ReceiptDate).ToList();
                case "TotalAmount":
                    return list.OrderBy(item => item.TotalAmount).ToList();
                case "CompanyInfo":
                    return list.OrderBy(item => item.CompanyInfo).ToList();
                default:
                    return list;
            }
        }

        public List<ArticleInfo> fetchReceiptArticles(int idReceipt)
        {
            List<ArticleInfo> boughtArticles = new List<ArticleInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT R.*, A.* " +
                        "FROM Receipts AS R " +
                        "INNER JOIN ReceiptItems as RI ON R.ReceiptID = RI.ReceiptID " +
                        "INNER JOIN Articles AS A ON RI.ArticleID = A.ArticleID " +
                        "WHERE R.ReceiptID = @idReceipt";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@idReceipt", idReceipt);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ArticleInfo articleInfo = new ArticleInfo
                                {
                                    ArticleID = reader.GetInt32(reader.GetOrdinal("ArticleID")),
                                    ArticleName = reader.GetString(reader.GetOrdinal("ArticleName")),
                                    ArticlePrice = reader.GetDecimal(reader.GetOrdinal("ArticlePrice")),
                                    ArticleDescription = reader.GetString(reader.GetOrdinal("ArticleDescription"))
                                };

                                boughtArticles.Add(articleInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in fetching ReceiptArticles: " + ex.ToString());
            }
            return boughtArticles;
        }

        public void obtainFilteredReceipts()
        {
            int filteredBuyerID = 0;
            DateTime rngMin = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
            rngMin = rngMin.AddYears(1);
            DateTime rngMax = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;
            rngMax = rngMax.AddYears(-1);
            DateTime fromDate = rngMin;
            DateTime toDate = rngMax;

            if (!string.IsNullOrEmpty(BuyerIdFilter))
                filteredBuyerID = int.Parse(BuyerIdFilter);

            if (!string.IsNullOrEmpty(FromDateFilter.ToString()))
            {
                DateTime userFrom = DateTime.Parse(FromDateFilter.ToString());
                if (userFrom > rngMin)
                    fromDate = userFrom;
            }

            if (!string.IsNullOrEmpty(ToDateFilter.ToString()))
            {
                DateTime userTo = DateTime.Parse(ToDateFilter.ToString());
                if (userTo < rngMax)
                    toDate = userTo;
            }

            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=accountify;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Construct the SQL query with filters
                    string sql = "SELECT * FROM Receipts WHERE 1=1"; // Initial SQL query

                    // check for BuyerID filter
                    if (filteredBuyerID != 0)
                    {
                        sql += " AND BuyerID = @filteredBuyerID";
                    }
                    // apply date filters
                    sql += " AND ReceiptDate >= @FromDate AND ReceiptDate <= @ToDate";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (filteredBuyerID != 0)
                            command.Parameters.AddWithValue("@filteredBuyerID", filteredBuyerID);
                        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
                        command.Parameters.AddWithValue("@ToDate", toDate.Date);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            listReceipts.Clear(); // Clear the existing data
                            while (reader.Read())
                            {
                                ReceiptInfo receiptInfo = new ReceiptInfo
                                {
                                    ReceiptID = reader.GetInt32(reader.GetOrdinal("ReceiptID")),
                                    BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                    BuyerName = reader.GetString(reader.GetOrdinal("BuyerName")),
                                    BuyerAddress = reader.GetString(reader.GetOrdinal("BuyerAddress")),
                                    ReceiptDate = reader.GetDateTime(reader.GetOrdinal("ReceiptDate")).ToString("dd/MM/yyyy"),
                                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                    CompanyInfo = reader.GetString(reader.GetOrdinal("CompanyInfo"))
                                };
                                receiptInfo.ReceiptArticles = fetchReceiptArticles(receiptInfo.ReceiptID);

                                listReceipts.Add(receiptInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
        }
        public void obtainDefaultReceipts()
        {
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=accountify;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT * FROM Receipts";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ReceiptInfo receiptInfo = new ReceiptInfo
                                {
                                    ReceiptID = reader.GetInt32(reader.GetOrdinal("ReceiptID")),
                                    BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                    BuyerName = reader.GetString(reader.GetOrdinal("BuyerName")),
                                    BuyerAddress = reader.GetString(reader.GetOrdinal("BuyerAddress")),
                                    ReceiptDate = reader.GetDateTime(reader.GetOrdinal("ReceiptDate")).ToString("dd/MM/yyyy"),
                                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                    CompanyInfo = reader.GetString(reader.GetOrdinal("CompanyInfo"))
                                };
                                receiptInfo.ReceiptArticles = fetchReceiptArticles(receiptInfo.ReceiptID);

                                listReceipts.Add(receiptInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
        }

        // FilterReceipts
        public IActionResult OnPost(string id)
        {
            obtainFilteredReceipts();
            return Page();
        }
        public IActionResult OnGet(string sort)
        {
            obtainDefaultReceipts();
            // Check if a sorting column is provided
            if (!string.IsNullOrEmpty(sort))
            {
                SortColumn = sort;
                listReceipts = SortList(listReceipts, SortColumn);
            }

            return Page();
        }
    }
}