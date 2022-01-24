using AdapterCommand.Entity;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdapterCommand
{
    internal class Program
    {

        // Đọc kiếm tra đường dẫn

        public class MyHttpClientHandler : HttpClientHandler
        {
            public MyHttpClientHandler(CookieContainer cookie_container)
            {

                CookieContainer = cookie_container;     // Thay thế CookieContainer mặc định
                AllowAutoRedirect = false;                // không cho tự động Redirect
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                UseCookies = true;
            }
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                         CancellationToken cancellationToken)
            {
                Console.WriteLine("Bất đầu kết nối " + request.RequestUri.ToString());
                // Thực hiện truy vấn đến Server
                var response = await base.SendAsync(request, cancellationToken);
                var host = request.RequestUri.Host.ToLower();
                if(host.Contains("google.com")|| host.Contains("github.com")|| host.Contains("facebook.com"))
                {
                   
                }
                else
                {
                    Console.WriteLine("Hoàn thành tải dữ liệu");
                }
               
                return response;
            }
        }

        public class ChangeUri : DelegatingHandler
        {
            public ChangeUri(HttpMessageHandler innerHandler) : base(innerHandler) { }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                   CancellationToken cancellationToken)
            {
                var host = request.RequestUri.Host.ToLower();
                Console.WriteLine($"Check in  ChangeUri - {host}");
                if (host.Contains("google.com"))
                {
                    // Đổi địa chỉ truy cập từ google.com sang github
                    request.RequestUri = new Uri("https://github.com/");
                }
                // Chuyển truy vấn cho base (thi hành InnerHandler)
                return base.SendAsync(request, cancellationToken);
            }
        }


        public class DenyAccessFacebook : DelegatingHandler
        {
            public DenyAccessFacebook(HttpMessageHandler innerHandler) : base(innerHandler) { }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                         CancellationToken cancellationToken)
            {
                var host = request.RequestUri.Host.ToLower();
                Console.WriteLine($"Check in DenyAccessFacebook - {host}");
                if (host.Contains("facebook.com"))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(Encoding.UTF8.GetBytes("Không được truy cập"));
                    return await Task.FromResult<HttpResponseMessage>(response);
                }
                // Chuyển truy vấn cho base (thi hành InnerHandler)
                return await base.SendAsync(request, cancellationToken);
            }
        }
        static void ShowDataTable(DataTable table)
        {
            Console.WriteLine("Bang san pham: " + table.TableName);
            // Hiện thị các cột
            Console.Write($"{"Index",15}");

            foreach (DataColumn column in table.Columns)
            {
                Console.Write($"{column.ColumnName,15}");
            }
            Console.WriteLine();

            // Hiện thị các dòng dữ liệu
            int number_cols = table.Columns.Count;
            int IndexContent = 0;
            int index = 0;
            foreach (DataRow row in table.Rows)
            {
                Console.Write($"{index,15}");
                for (int i = 0; i < number_cols; i++)
                {
                    Console.Write($"{row[i],15}");
                }
                index = index + 1;
                Console.WriteLine();
            }

        }
        //static async void ShowDataTable()
        //{
        //    var listproduct = new List<Product>();
        //    var datatable = new DataTable();
        //    datatable.Columns.Add("Id");
        //    datatable.Columns.Add("Name");
        //    datatable.Columns.Add("Price");
        //    datatable.Columns.Add("LastPrice");
        //    string sql = "SELECT * FROM Products";
        //    var sqlconnectstring = "Data Source=ADMIN\\SQLEXPRESS;Initial Catalog=DapperContent;Integrated Security=True;";
        //    using (var connection = new SqlConnection(sqlconnectstring))
        //    {
        //        connection.Open();
        //        var orderDetail = await connection.QueryAsync<List<Product>>(sql);
        //    }
        //    foreach(var sub in listproduct)
        //    {
        //        datatable.Rows.Add(sub.Id, sub.Name, sub.Price, sub.LastPrice, sub.CategoryId);
        //    }    

        //    Console.WriteLine("Bang san pham: ");
        //    // Hiện thị các cột
        //    foreach (DataColumn column in datatable.Columns)
        //    {
        //        Console.Write($"{column.ColumnName,15}");
        //    }
        //    Console.WriteLine();

        //    // Hiện thị các dòng dữ liệu
        //    int number_cols = datatable.Columns.Count;
        //    foreach (DataRow row in datatable.Rows)
        //    {
        //        for (int i = 0; i < number_cols; i++)
        //        {
        //            Console.Write($"{row[i],15}");
        //        }
        //        Console.WriteLine();
        //    }
        //}

        static void ShowDataTableCate(DataTable table)
        {
            Console.WriteLine("Bang danh muc san pham: " + table.TableName);
            // Hiện thị các cột
            Console.Write($"{"Index",15}");

            foreach (DataColumn column in table.Columns)
            {
                Console.Write($"{column.ColumnName,15}");
            }
            Console.WriteLine();

            // Hiện thị các dòng dữ liệu
            int number_cols = table.Columns.Count;
            int index = 0;
            foreach (DataRow row in table.Rows)
            {
                Console.Write($"{index,15}");
                for (int i = 0; i < number_cols; i++)
                {
                    Console.Write($"{row[i],15}");
                }
                index++;
                Console.WriteLine();
            }

        }
        static Product FindById(int Id)
        {
            string sql = "SELECT Id,Name,Price,LastPrice,CategoryId FROM Products where Id = @Id";
            var sqlconnectstring = "Data Source=ADMIN\\SQLEXPRESS;Initial Catalog=DapperContent;Integrated Security=True;";
            using (var connection = new SqlConnection(sqlconnectstring))
            {
                connection.Open();
                var orderDetail = connection.QueryFirst<Product>(sql, new { Id = Id });
                var product = orderDetail as Product;
                return product;
            }
        }
        static void AddProduct(DataTable dataTable, string name, decimal price, decimal lastprice, int categoryId)
        {

            var rowadd = dataTable.Rows.Add();
            rowadd["Name"] = name;
            rowadd["Price"] = price;
            rowadd["LastPrice"] = lastprice;
            rowadd["CategoryId"] = categoryId;

        }
        static async void Delete(DataTable dataTable, int Id)
        {
            //string sql = "Delete FROM Products where Id = @Id";
            //var sqlconnectstring = "Data Source=ADMIN\\SQLEXPRESS;Initial Catalog=DapperContent;Integrated Security=True;";
            //using (var connection = new SqlConnection(sqlconnectstring))
            //{
            //    connection.Open();
            //    var orderDetail = connection.Execute(sql, new { Id = Id });
            //    Console.WriteLine("Xoa thanh cong");

            //}
            var rowdelete = dataTable.Rows[Id];
            rowdelete.Delete();

        }
        static void Update(DataTable dataTable, string name, decimal price, decimal lasprice, int categoryId, int Index)
        {
            var rowedit = dataTable.Rows[Index];
            rowedit["Name"] = name;
            rowedit["Price"] = price;
            rowedit["LastPrice"] = lasprice;
            rowedit["CategoryId"] = categoryId;
        }

        static async Task DownloadImage(byte[] bytes, DataTable dataTable)
        {
            int dem = 0;
            var sqlconnectstring = "Data Source=ADMIN\\SQLEXPRESS;Initial Catalog=DapperContent;Integrated Security=True;";
            using (var connection = new SqlConnection(sqlconnectstring))
            {
                connection.Open();
                var listItem = await connection.QueryAsync<List<Product>>("select*from Products");
                foreach(var item in listItem)
                {
                    dem++;
                }    
            }
            string filepath = $"anh{dem}.png";
            using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            var rowedit = dataTable.Rows[dem-1];
            rowedit["PathImage"] = filepath;
             
        }
        static async Task UpdateImage(byte[] bytes, int index, DataTable dataTable)
        {
            int dem = 0;
            var sqlconnectstring = "Data Source=ADMIN\\SQLEXPRESS;Initial Catalog=DapperContent;Integrated Security=True;";
            using (var connection = new SqlConnection(sqlconnectstring))
            {
                connection.Open();
                var listItem = await connection.QueryAsync<List<Product>>("select*from Products");
                foreach (var item in listItem)
                {
                    dem++;
                }
            }
            string filepath = $"anh{dem}.png";
            using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            var rowedit = dataTable.Rows[index];
            rowedit["PathImage"] = filepath;

        }

        static void GetDetailCategory(int Id)
        {
            string sql = "SELECT * FROM Categories AS A INNER JOIN Products AS B ON A.Id = B.CategoryId;";
            var sqlconnectstring = "Data Source=ADMIN\\SQLEXPRESS;Initial Catalog=DapperContent;Integrated Security=True;";
            using (var connection = new SqlConnection(sqlconnectstring))
            {
                var orderDictionary = new Dictionary<int, Category>();


                var list = connection.Query<Category, Product, Category>(
                sql,
                (category, productofcategory) =>
                {
                    Category categoryEntry;

                    if (!orderDictionary.TryGetValue(category.Id, out categoryEntry))
                    {
                        categoryEntry = category;
                        categoryEntry.Products = new List<Product>();
                        orderDictionary.Add(categoryEntry.Id, categoryEntry);
                    }

                    categoryEntry.Products.Add(productofcategory);
                    return categoryEntry;
                },
                splitOn: "Id")
                .Distinct()
                .ToList();
                int total = 0;
                foreach (var sub in list)
                {
                    if (sub.Id == Id)
                    {
                        foreach (var product in sub.Products)
                        {
                            Console.WriteLine($"Name :{product.Name}      Price: {product.Price}      LastPrice : {product.LastPrice}         CategoryId : {product.CategoryId}");
                            total++;
                        }
                        Console.WriteLine($"Tong san pham trong danh muc {sub.Name} La {total}");
                    }
                }

            }
        }

        static async Task Main(string[] args)
        {
            bool status = true;
            var sqlconnectstring = "Data Source=ADMIN\\SQLEXPRESS;Initial Catalog=DapperContent;Integrated Security=True;";
            var connection = new SqlConnection(sqlconnectstring);
            connection.Open();
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.TableMappings.Add("Table", "Products");
            // SelectCommand - Thực thi khi gọi Fill lấy dữ liệu về DataSet
            adapter.SelectCommand = new SqlCommand("select Id,Name,Price,LastPrice,CategoryId,PathImage from Products", connection);
            // InsertCommand - Thực khi khi gọi Update, nếu DataSet có chèn dòng mới (vào DataTable)
            // Dữ liệu lấy từ DataTable, như cột Ten tương  ứng với tham số 
            adapter.InsertCommand = new SqlCommand(@"INSERT INTO Products (Name, Price,LastPrice,CategoryId,PathImage) VALUES (@Name, @Price,@LastPrice,@CategoryId,@PathImage)", connection);
            adapter.InsertCommand.Parameters.Add("@PathImage", SqlDbType.NVarChar, 50, "PathImage");
            adapter.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 50, "Name");
            adapter.InsertCommand.Parameters.Add("@Price", SqlDbType.Decimal, 50, "Price");
            adapter.InsertCommand.Parameters.Add("@LastPrice", SqlDbType.Decimal, 50, "LastPrice");
            var pr4 = adapter.InsertCommand.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int));
            pr4.SourceColumn = "CategoryId";
            pr4.SourceVersion = DataRowVersion.Original;  // Giá trị ban đầu
                                                          // DeleteCommand  - Thực thi khi gọi Update, nếu có remove dòng nào đó của DataTable
            adapter.DeleteCommand = new SqlCommand(@"DELETE FROM Products WHERE Id = @Id", connection);
            var pr1 = adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
            pr1.SourceColumn = "Id";
            pr1.SourceVersion = DataRowVersion.Original;  // Giá trị ban đầu


            // UpdateCommand -  Thực thi khi gọi Update, nếu có chỉnh sửa trường dữ liệu nào đó
            adapter.UpdateCommand = new SqlCommand(@"update Products SET Name=@Name, Price = @Price,LastPrice = @LastPrice,CategoryId = @CategoryId,PathImage = @PathImage
                                         WHERE Id = @Id", connection);
            adapter.UpdateCommand.Parameters.Add("@PathImage", SqlDbType.NVarChar, 255, "PathImage");
            adapter.UpdateCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 255, "Name");
            adapter.UpdateCommand.Parameters.Add("@Price", SqlDbType.NVarChar, 255, "Price");
            adapter.UpdateCommand.Parameters.Add("@LastPrice", SqlDbType.NVarChar, 255, "LastPrice");
            var pr3 = adapter.UpdateCommand.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int));
            pr3.SourceColumn = "CategoryId";
            pr3.SourceVersion = DataRowVersion.Original;  // Giá trị ban đầu

            var pr2 = adapter.UpdateCommand.Parameters.Add(
                    new SqlParameter("@Id", SqlDbType.Int)
                    { SourceColumn = "Id" });
            pr2.SourceVersion = DataRowVersion.Original;

            DataSet dataSet = new DataSet();

            adapter.Fill(dataSet);
            // Lấy DataTable kết quả và hiện thị
            DataTable dataTable = dataSet.Tables["Products"];
            var reader = connection.ExecuteReader("SELECT * FROM Categories;");



            DataTable table = new DataTable();
            table.Load(reader);
            ShowDataTable(dataTable);
            ShowDataTableCate(table);
            while (status)
            {
                Console.WriteLine("Nhan 1 them sam pham");
                Console.WriteLine("Nhan 2 cap nhap san pham");
                Console.WriteLine("Nhan 3 xoa san pham");
                Console.WriteLine("Nhan 4 tim kiem san pham theo Id");
                Console.WriteLine("Nhan 5 tim kiem san pham trong danh muc");
                Console.WriteLine("Nhan 6 cap nhap hinh anh");
                Console.WriteLine("Nhan 7 thoat chuong trinh  ");
                Console.WriteLine("Nhan so can thuc hien");
                int request = int.Parse(Console.ReadLine());
                switch (request)
                {
                    case 1:
                        {
                            Console.WriteLine("Nhap ten san pham : ");
                            var name = Console.ReadLine();
                            Console.WriteLine("Nhap gia san pham : ");
                            var price = (decimal)int.Parse(Console.ReadLine());
                            Console.WriteLine("Nhap gia cuoi san pham : ");
                            var lastprice = (decimal)int.Parse(Console.ReadLine());
                            Console.WriteLine("Nhap anh cua san pham : ");
                            var pathimage = Console.ReadLine();
                            Console.WriteLine("Nhap loai san pham : ");
                            var categoyId = int.Parse(Console.ReadLine());


                            AddProduct(dataTable, name, price, lastprice, categoyId);
                            
                            adapter.Update(dataSet);


                            CookieContainer cookies = new CookieContainer();

                            // TẠO CHUỖI HANDLER
                            var bottomHandler = new MyHttpClientHandler(cookies);              // handler đáy (cuối)
                            var changeUriHandler = new ChangeUri(bottomHandler);
                            var denyAccessFacebook = new DenyAccessFacebook(changeUriHandler); // handler đỉnh

                            using var httpclient = new HttpClient(denyAccessFacebook);
                            using var httprequestmessage = new HttpRequestMessage();
                            httprequestmessage.Method = HttpMethod.Get;
                            httprequestmessage.RequestUri = new Uri(pathimage);
                            var read = await httpclient.SendAsync(httprequestmessage);
                            //var readstring = await read.Content.ReadAsByteArrayAsync();
                            var htmltextcontent = await read.Content.ReadAsStringAsync();

                            var htmltext = await read.Content.ReadAsByteArrayAsync();
                            await DownloadImage(htmltext, dataTable);
                            adapter.Update(dataSet);
                            ShowDataTable(dataTable);
                            break;
                        }
                    case 2:
                        {
                            Console.WriteLine("Nhap Index cua san pham can cap nhap Id : ");
                            var indexUpdate = int.Parse(Console.ReadLine());
                            Console.WriteLine("Nhap ten san pham : ");
                            var name = Console.ReadLine();
                            Console.WriteLine("Nhap gia san pham : ");
                            var price = (decimal)int.Parse(Console.ReadLine());
                            Console.WriteLine("Nhap gia cuoi san pham : ");
                            var lastprice = (decimal)int.Parse(Console.ReadLine());
                            Console.WriteLine("Nhap anh cua san pham : ");
                            var pathimage = Console.ReadLine();
                            Console.WriteLine("Nhap loai san pham : ");
                            var categoyId = int.Parse(Console.ReadLine());
                            Update(dataTable, name, price, lastprice, categoyId, indexUpdate);
                            adapter.Update(dataSet);

                            CookieContainer cookies = new CookieContainer();

                            // TẠO CHUỖI HANDLER
                            var bottomHandler = new MyHttpClientHandler(cookies);              // handler đáy (cuối)
                            var changeUriHandler = new ChangeUri(bottomHandler);
                            var denyAccessFacebook = new DenyAccessFacebook(changeUriHandler); // handler đỉnh

                            using var httpclient = new HttpClient(denyAccessFacebook);
                            using var httprequestmessage = new HttpRequestMessage();
                            httprequestmessage.Method = HttpMethod.Get;
                            httprequestmessage.RequestUri = new Uri(pathimage);
                            var read = await httpclient.SendAsync(httprequestmessage);
                            //var readstring = await read.Content.ReadAsByteArrayAsync();
                            var htmltextcontent = await read.Content.ReadAsStringAsync();

                            var htmltext = await read.Content.ReadAsByteArrayAsync();
                            await UpdateImage(htmltext, indexUpdate, dataTable);
                            adapter.Update(dataSet);
                            ShowDataTable(dataTable);
                            break;
                        }
                    case 3:
                        {
                            Console.WriteLine("Nhap Index cua san pham can xoa : ");
                            var index = int.Parse(Console.ReadLine());
                            Delete(dataTable, index);
                            adapter.Update(dataSet);
                            ShowDataTable(dataTable);
                            break;
                        }
                    case 4:
                        {
                            Console.WriteLine("Nhap Id cua san pham Id : ");
                            var Id = int.Parse(Console.ReadLine());
                            var list = FindById(Id);

                            var product = list;
                            Console.WriteLine($"Name :{product.Name}      Price: {product.Price}      LastPrice : {product.LastPrice}         CategoryId : {product.CategoryId}");
                            break;
                        }
                    case 5:
                        {
                            Console.WriteLine("Nhap Id cua dang muc san pham Id : ");
                            var Id = int.Parse(Console.ReadLine());
                            GetDetailCategory(Id);
                            break;
                        }
                  
                    case 6:
                        {
                            Console.WriteLine("Nhap Index san pham can them hinh anh");
                            var id = int.Parse(Console.ReadLine());
                            Console.WriteLine("Nhap dia chi hinh anh image : ");
                            string url = Console.ReadLine();

                            CookieContainer cookies = new CookieContainer();

                            // TẠO CHUỖI HANDLER
                            var bottomHandler = new MyHttpClientHandler(cookies);              // handler đáy (cuối)
                            var changeUriHandler = new ChangeUri(bottomHandler);
                            var denyAccessFacebook = new DenyAccessFacebook(changeUriHandler); // handler đỉnh

                            using var httpclient = new HttpClient(denyAccessFacebook);
                            using var httprequestmessage = new HttpRequestMessage();
                            httprequestmessage.Method = HttpMethod.Get;
                            httprequestmessage.RequestUri = new Uri(url);
                            var read = await httpclient.SendAsync(httprequestmessage);
                            //var readstring = await read.Content.ReadAsByteArrayAsync();
                            var htmltextcontent = await read.Content.ReadAsStringAsync();

                            var htmltext = await read.Content.ReadAsByteArrayAsync();
                            await  UpdateImage(htmltext,id,dataTable);
                            adapter.Update(dataSet);
                            ShowDataTable(dataTable);
                            //Console.WriteLine(htmltextcontent);
                            break;
                        }
                    case 7:
                        {
                            status = false;
                            Console.WriteLine("Ban da thoat chuong trinh");
                            Console.ReadLine();
                            break;
                        }

                }
            }

            connection.Close();


        }
    }
}
