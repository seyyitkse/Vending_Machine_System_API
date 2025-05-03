using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vending.ApiLayer.Services.Abstract;
using Vending.ApiLayer.Services.Concrete;
using Vending.DataAccessLayer.Concrete;
using Vending.DtoLayer.Dtos.OrderDto;
using Vending.EntityLayer.Concrete;
namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly VendingContext _context;
        private readonly ILogger<OrderController> _logger;
        private readonly IBackgroundTaskService _backgroundTaskService;
        private readonly BalanceService _balanceService;
        public OrderController(VendingContext context, ILogger<OrderController> logger, IBackgroundTaskService backgroundTaskService, BalanceService balanceService)
        {
            _context = context;
            _logger = logger;
            _backgroundTaskService = backgroundTaskService;
            _balanceService = balanceService;
        }


        // GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            _logger.LogInformation("Tüm siparişler getiriliyor.");
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            _logger.LogInformation("Sipariş getiriliyor: {OrderId}", id);
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Sipariş bulunamadı: {OrderId}", id);
                return NotFound();
            }

            return order;
        }

        // POST: api/Order
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            _logger.LogInformation("Yeni sipariş oluşturuluyor.");
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Yeni sipariş oluşturuldu: {OrderId}", order.OrderId);
            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }

        // PUT: api/Order/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.OrderId)
            {
                _logger.LogWarning("Sipariş ID uyuşmazlığı: {OrderId}", id);
                return BadRequest();
            }

            _logger.LogInformation("Sipariş güncelleniyor: {OrderId}", id);
            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Sipariş güncellendi: {OrderId}", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    _logger.LogWarning("Sipariş bulunamadı: {OrderId}", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            _logger.LogInformation("Sipariş siliniyor: {OrderId}", id);
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Sipariş bulunamadı: {OrderId}", id);
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sipariş silindi: {OrderId}", id);
            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

        //[HttpPost("sell")]
        //public async Task<ActionResult<Order>> SellProduct(CreateOrderDto orderDto)
        //{
        //    _logger.LogInformation("Ürün satışı gerçekleştiriliyor.");

        //    var product = await _context.Products.FindAsync(orderDto.ProductId);
        //    if (product == null)
        //    {
        //        _logger.LogWarning("Ürün bulunamadı: {ProductId}", orderDto.ProductId);
        //        return NotFound(new { message = "Ürün bulunamadı." });
        //    }

        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserCode == orderDto.UserCode);
        //    if (user == null)
        //    {
        //        _logger.LogWarning("Kullanıcı bulunamadı: {UserCode}", orderDto.UserCode);
        //        return NotFound(new { message = "Kullanıcı bulunamadı." });
        //    }

        //    if (product.Stock < orderDto.Quantity)
        //    {
        //        _logger.LogWarning("Yetersiz stok: {ProductId}", orderDto.ProductId);
        //        return BadRequest(new { message = "Yetersiz stok." });
        //    }

        //    product.Stock -= orderDto.Quantity;
        //    product.IsCriticalStock = product.Stock < 5;

        //    _context.Products.Update(product);

        //    var order = new Order
        //    {
        //        ProductId = orderDto.ProductId,
        //        Quantity = orderDto.Quantity,
        //        UserCode = orderDto.UserCode,
        //        VendId = orderDto.VendId,
        //        Product = product,
        //        Vend = await _context.Vends.FindAsync(orderDto.VendId),
        //        OrderDate = DateTime.UtcNow, // Sipariş tarihini ekledik
        //        TotalPrice = product.Price * orderDto.Quantity // Sipariş tutarını hesapladık
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    _logger.LogInformation("Ürün satışı gerçekleştirildi: {OrderId} - Tarih: {OrderDate} - Toplam Tutar: {TotalPrice}",
        //        order.OrderId, order.OrderDate, order.TotalPrice);

        //    return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        //}
        //[HttpPost("sell")]
        //public async Task<ActionResult<Order>> SellProduct(CreateOrderDto orderDto)
        //{
        //    _logger.LogInformation("Ürün satışı gerçekleştiriliyor.");

        //    var product = await _context.Products.FindAsync(orderDto.ProductId);
        //    if (product == null)
        //    {
        //        _logger.LogWarning("Ürün bulunamadı: {ProductId}", orderDto.ProductId);
        //        return NotFound(new { message = "Ürün bulunamadı." });
        //    }

        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserCode == orderDto.UserCode);
        //    if (user == null)
        //    {
        //        _logger.LogWarning("Kullanıcı bulunamadı: {UserCode}", orderDto.UserCode);
        //        return NotFound(new { message = "Kullanıcı bulunamadı." });
        //    }

        //    if (product.Stock < orderDto.Quantity)
        //    {
        //        _logger.LogWarning("Yetersiz stok: {ProductId}", orderDto.ProductId);
        //        return BadRequest(new { message = "Yetersiz stok." });
        //    }

        //    product.Stock -= orderDto.Quantity;
        //    product.IsCriticalStock = product.Stock < 5;
        //    _context.Products.Update(product);

        //    var vend = await _context.Vends.FindAsync(orderDto.VendId);

        //    var order = new Order
        //    {
        //        ProductId = orderDto.ProductId,
        //        Quantity = orderDto.Quantity,
        //        UserCode = orderDto.UserCode,
        //        VendId = orderDto.VendId,
        //        Product = product,
        //        Vend = vend,
        //        OrderDate = DateTime.UtcNow,
        //        TotalPrice = product.Price * orderDto.Quantity
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    // 🧾 Fatura PDF oluşturma ve e-posta gönderme işlemi
        //    try
        //    {
        //        var invoiceModel = new InvoiceViewModel
        //        {
        //            OrderId = order.OrderId,
        //            UserName = user.UserName,
        //            Email = user.Email,
        //            ProductName = product.Name,
        //            Quantity = order.Quantity,
        //            Price = product.Price,
        //            TotalPrice = order.TotalPrice,
        //            OrderDate = order.OrderDate,
        //            VendName = vend?.Name ?? "Bilinmiyor"
        //        };

        //        var invoiceService = new InvoiceService(); // Razor + DinkToPdf
        //        var pdfBytes = await invoiceService.GenerateInvoicePdfAsync(invoiceModel);

        //        var emailService = new EmailService(); // SMTP ile gönderim
        //        await emailService.SendInvoiceEmailAsync(user.Email, user.UserName, pdfBytes);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Fatura oluşturulurken veya mail gönderilirken bir hata oluştu.");
        //        // İsteğe bağlı: Kullanıcıya hata dönme, loglama yeterli
        //    }

        //    _logger.LogInformation("Ürün satışı tamamlandı: {OrderId} - {TotalPrice}", order.OrderId, order.TotalPrice);

        //    return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        //}

        //[HttpPost("sell")]
        //public async Task<ActionResult<Order>> SellProduct(CreateOrderDto orderDto)
        //{
        //    _logger.LogInformation("Ürün satışı gerçekleştiriliyor.");

        //    var product = await _context.Products.FindAsync(orderDto.ProductId);
        //    if (product == null)
        //    {
        //        _logger.LogWarning("Ürün bulunamadı: {ProductId}", orderDto.ProductId);
        //        return NotFound(new { message = "Ürün bulunamadı." });
        //    }

        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserCode == orderDto.UserCode);
        //    if (user == null)
        //    {
        //        _logger.LogWarning("Kullanıcı bulunamadı: {UserCode}", orderDto.UserCode);
        //        return NotFound(new { message = "Kullanıcı bulunamadı." });
        //    }

        //    if (product.Stock < orderDto.Quantity)
        //    {
        //        _logger.LogWarning("Yetersiz stok: {ProductId}", orderDto.ProductId);
        //        return BadRequest(new { message = "Yetersiz stok." });
        //    }

        //    product.Stock -= orderDto.Quantity;
        //    product.IsCriticalStock = product.Stock < 5;
        //    _context.Products.Update(product);

        //    var vend = await _context.Vends.FindAsync(orderDto.VendId);

        //    var order = new Order
        //    {
        //        ProductId = orderDto.ProductId,
        //        Quantity = orderDto.Quantity,
        //        UserCode = orderDto.UserCode,
        //        VendId = orderDto.VendId,
        //        Product = product,
        //        Vend = vend,
        //        OrderDate = DateTime.UtcNow,
        //        TotalPrice = product.Price * orderDto.Quantity
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    // 🧾 Fatura PDF oluşturma ve e-posta gönderme işlemi
        //    try
        //    {
        //        var invoiceModel = new InvoiceViewModel
        //        {
        //            OrderId = order.OrderId,
        //            UserName = user.UserName,
        //            Email = user.Email,
        //            ProductName = product.Name,
        //            Quantity = order.Quantity,
        //            Price = product.Price,
        //            TotalPrice = order.TotalPrice,
        //            OrderDate = order.OrderDate,
        //            VendName = vend?.Name ?? "Bilinmiyor"
        //        };

        //        // Arka planda çalıştırma
        //        _ = Task.Run(async () =>
        //        {
        //            try
        //            {
        //                _logger.LogInformation("Fatura gönderme işlemi başlatılıyor...");
        //                await _backgroundTaskService.GenerateInvoiceAndSendEmailAsync(invoiceModel);
        //                _logger.LogInformation("Fatura başarıyla gönderildi.");
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, "Arka planda fatura gönderilirken hata oluştu.");
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Fatura oluşturulurken veya mail gönderilirken bir hata oluştu.");
        //        // İsteğe bağlı: Kullanıcıya hata dönme, loglama yeterli
        //    }

        //    _logger.LogInformation("Ürün satışı tamamlandı: {OrderId} - {TotalPrice}", order.OrderId, order.TotalPrice);

        //    return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        //}

        //[HttpPost("sell")]
        //public async Task<ActionResult<Order>> SellProduct(CreateOrderDto orderDto)
        //{
        //    _logger.LogInformation("Ürün satışı gerçekleştiriliyor.");

        //    var product = await _context.Products.FindAsync(orderDto.ProductId);
        //    if (product == null)
        //    {
        //        _logger.LogWarning("Ürün bulunamadı: {ProductId}", orderDto.ProductId);
        //        return NotFound(new { message = "Ürün bulunamadı." });
        //    }

        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserCode == orderDto.UserCode);
        //    if (user == null)
        //    {
        //        _logger.LogWarning("Kullanıcı bulunamadı: {UserCode}", orderDto.UserCode);
        //        return NotFound(new { message = "Kullanıcı bulunamadı." });
        //    }

        //    if (product.Stock < orderDto.Quantity)
        //    {
        //        _logger.LogWarning("Yetersiz stok: {ProductId}", orderDto.ProductId);
        //        return BadRequest(new { message = "Yetersiz stok." });
        //    }

        //    product.Stock -= orderDto.Quantity;
        //    product.IsCriticalStock = product.Stock < 5;
        //    _context.Products.Update(product);

        //    var vend = await _context.Vends.FindAsync(orderDto.VendId);

        //    var order = new Order
        //    {
        //        ProductId = orderDto.ProductId,
        //        Quantity = orderDto.Quantity,
        //        UserCode = orderDto.UserCode,
        //        VendId = orderDto.VendId,
        //        Product = product,
        //        Vend = vend,
        //        OrderDate = DateTime.UtcNow,
        //        TotalPrice = product.Price * orderDto.Quantity
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    _logger.LogInformation("Ürün satışı tamamlandı: {OrderId} - {TotalPrice}", order.OrderId, order.TotalPrice);

        //    // Fatura gönderme endpoint'ine istek atma
        //    //using (var httpClient = new HttpClient())
        //    //{
        //    //    var response = await httpClient.PostAsync($"https://localhost:44395/api/Order/send-invoice/{order.OrderId}", null);
        //    //    if (response.IsSuccessStatusCode)
        //    //    {
        //    //        _logger.LogInformation("Fatura gönderme işlemi başarıyla başlatıldı: {OrderId}", order.OrderId);
        //    //    }
        //    //    else
        //    //    {
        //    //        _logger.LogWarning("Fatura gönderme işlemi başarısız oldu: {OrderId}", order.OrderId);
        //    //    }
        //    //}

        //    return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        //}
        /// <summary>
        /// Ürün satışı gerçekleştirir.
        /// </summary>
        /// <param name="orderDto">Satış için gerekli sipariş bilgileri.</param>
        /// <returns>Oluşturulan sipariş.</returns>
        [HttpPost("sell")]
        public async Task<ActionResult<Order>> SellProduct(CreateOrderDto orderDto)
        {
            _logger.LogInformation("Ürün satışı gerçekleştiriliyor.");

            var product = await _context.Products.FindAsync(orderDto.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Ürün bulunamadı: {ProductId}", orderDto.ProductId);
                return NotFound(new { message = "Ürün bulunamadı." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserCode == orderDto.UserCode);
            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı: {UserCode}", orderDto.UserCode);
                return NotFound(new { message = "Kullanıcı bulunamadı." });
            }

            if (product.Stock < orderDto.Quantity)
            {
                _logger.LogWarning("Yetersiz stok: {ProductId}", orderDto.ProductId);
                return BadRequest(new { message = "Yetersiz stok." });
            }
            orderDto.Quantity = 1;
            var totalPrice = product.Price * orderDto.Quantity;

            if (!await _balanceService.HasSufficientBalanceAsync(orderDto.UserCode, totalPrice))
            {
                return BadRequest(new { message = "Yetersiz bakiye." });
            }

            await _balanceService.DeductBalanceAsync(orderDto.UserCode, totalPrice);

            product.Stock -= orderDto.Quantity;
            product.IsCriticalStock = product.Stock < 5;
            _context.Products.Update(product);

            var vend = await _context.Vends.FindAsync(orderDto.VendId);

            var order = new Order
            {
                ProductId = orderDto.ProductId,
                Quantity = orderDto.Quantity,
                UserCode = orderDto.UserCode,
                VendId = orderDto.VendId,
                Product = product,
                Vend = vend,
                OrderDate = DateTime.UtcNow,
                TotalPrice = totalPrice
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ürün satışı tamamlandı: {OrderId} - {TotalPrice}", order.OrderId, order.TotalPrice);

            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }




        [HttpPost("send-invoice/{orderId}")]
        public async Task<IActionResult> SendInvoice(int orderId)
        {
            _logger.LogInformation("Fatura gönderme işlemi başlatılıyor: {OrderId}", orderId);

            var order = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Vend)
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                _logger.LogWarning("Sipariş bulunamadı: {OrderId}", orderId);
                return NotFound(new { message = "Sipariş bulunamadı." });
            }

            if (order.IsInvoiceSent)
            {
                _logger.LogInformation("Fatura zaten gönderilmiş: {OrderId}", orderId);
                return BadRequest(new { message = "Fatura zaten gönderilmiş." });
            }

            var invoiceModel = new InvoiceViewModel
            {
                OrderId = order.OrderId,
                UserName = order.AppUser.UserName,
                Email = order.AppUser.Email,
                ProductName = order.Product.Name,
                Quantity = order.Quantity,
                Price = order.Product.Price,
                TotalPrice = order.TotalPrice,
                OrderDate = order.OrderDate,
                VendName = order.Vend?.Name ?? "Bilinmiyor"
            };

            try
            {
                await _backgroundTaskService.GenerateInvoiceAndSendEmailAsync(invoiceModel);
                order.IsInvoiceSent = true; // Fatura gönderildikten sonra true olarak ayarlanır
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Fatura başarıyla gönderildi: {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura gönderilirken hata oluştu: {OrderId}", orderId);
                return StatusCode(500, new { message = "Fatura gönderilirken bir hata oluştu." });
            }

            return Ok(new { message = "Fatura başarıyla gönderildi." });
        }

        [HttpPost("send-oldest-unpaid-invoice")]
        public async Task<IActionResult> SendOldestUnpaidInvoice()
        {
            try
            {
                // En eski gönderilmemiş siparişi getir
                var pendingOrder = await _context.Orders
                    .Include(o => o.Product)
                    .Include(o => o.Vend)
                    .Include(o => o.AppUser)
                    .Where(o => !o.IsInvoiceSent)
                    .OrderBy(o => o.OrderDate)
                    .FirstOrDefaultAsync();

                if (pendingOrder == null)
                {
                    _logger.LogInformation("Gönderilmemiş fatura bulunamadı.");
                    return Ok(new
                    {
                        success = false,
                        message = "Gönderilmemiş fatura bulunamadı."
                    });
                }

                // Fatura modeli oluştur
                var invoiceModel = new InvoiceViewModel
                {
                    OrderId = pendingOrder.OrderId,
                    UserName = pendingOrder.AppUser.UserName,
                    Email = pendingOrder.AppUser.Email,
                    ProductName = pendingOrder.Product.Name,
                    Quantity = pendingOrder.Quantity,
                    Price = pendingOrder.Product.Price,
                    TotalPrice = pendingOrder.TotalPrice,
                    OrderDate = pendingOrder.OrderDate,
                    VendName = pendingOrder.Vend?.Name ?? "Bilinmiyor"
                };

                // Fatura oluştur ve e-posta ile gönder
                await _backgroundTaskService.GenerateInvoiceAndSendEmailAsync(invoiceModel);

                // Siparişi işaretle
                pendingOrder.IsInvoiceSent = true;
                _context.Orders.Update(pendingOrder);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Fatura başarıyla gönderildi. Sipariş ID: {OrderId}", pendingOrder.OrderId);

                // Başarılı yanıt dön
                return Ok(new
                {
                    success = true,
                    message = "Fatura başarıyla gönderildi.",
                    invoice = invoiceModel
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura gönderimi sırasında bir hata oluştu.");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Fatura gönderimi sırasında bir hata oluştu.",
                    error = ex.Message
                });
            }
        }



        //[HttpPost("send-missing-invoices")]
        //public async Task<IActionResult> SendMissingInvoices()
        //{
        //    _logger.LogInformation("Gönderilmemiş faturalar kontrol ediliyor...");

        //    var pendingOrders = await _context.Orders
        //        .Include(o => o.Product)
        //        .Include(o => o.Vend)
        //        .Include(o => o.AppUser)
        //        .Where(o => !o.IsInvoiceSent)
        //        .ToListAsync();

        //    if (!pendingOrders.Any())
        //    {
        //        _logger.LogInformation("Gönderilmemiş fatura bulunamadı.");
        //        return Ok(new { message = "Tüm faturalar daha önce gönderilmiş." });
        //    }

        //    foreach (var order in pendingOrders)
        //    {
        //        try
        //        {
        //            var invoiceModel = new InvoiceViewModel
        //            {
        //                OrderId = order.OrderId,
        //                UserName = order.AppUser.UserName,
        //                Email = order.AppUser.Email,
        //                ProductName = order.Product.Name,
        //                Quantity = order.Quantity,
        //                Price = order.Product.Price,
        //                TotalPrice = order.TotalPrice,
        //                OrderDate = order.OrderDate,
        //                VendName = order.Vend?.Name ?? "Bilinmiyor"
        //            };

        //            await _backgroundTaskService.GenerateInvoiceAndSendEmailAsync(invoiceModel);

        //            // Fatura başarıyla gönderildiyse işaretle
        //            order.IsInvoiceSent = true;
        //            _context.Orders.Update(order);
        //            _logger.LogInformation("Fatura gönderildi ve işaretlendi: {OrderId}", order.OrderId);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Fatura gönderimi sırasında hata oluştu: {OrderId}", order.OrderId);
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Fatura gönderimi tamamlandı." });
        //}
        [HttpPost("send-missing-invoices")]
        public async Task<IActionResult> SendMissingInvoices()
        {
            _logger.LogInformation("Gönderilmemiş faturalar kontrol ediliyor...");

            // İlk başta, gönderilmemiş ilk siparişi al
            var pendingOrder = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Vend)
                .Include(o => o.AppUser)
                .Where(o => !o.IsInvoiceSent)
                .OrderBy(o => o.OrderDate) // Eskiden yeniye sıralama
                .FirstOrDefaultAsync();

            if (pendingOrder == null)
            {
                _logger.LogInformation("Gönderilmemiş fatura bulunamadı.");
                return Ok(new { message = "Tüm faturalar daha önce gönderilmiş." });
            }

            try
            {
                var invoiceModel = new InvoiceViewModel
                {
                    OrderId = pendingOrder.OrderId,
                    UserName = pendingOrder.AppUser.UserName,
                    Email = pendingOrder.AppUser.Email,
                    ProductName = pendingOrder.Product.Name,
                    Quantity = pendingOrder.Quantity,
                    Price = pendingOrder.Product.Price,
                    TotalPrice = pendingOrder.TotalPrice,
                    OrderDate = pendingOrder.OrderDate,
                    VendName = pendingOrder.Vend?.Name ?? "Bilinmiyor"
                };

                await _backgroundTaskService.GenerateInvoiceAndSendEmailAsync(invoiceModel);

                // Fatura başarıyla gönderildiyse işaretle
                pendingOrder.IsInvoiceSent = true;
                _context.Orders.Update(pendingOrder);
                _logger.LogInformation("Fatura gönderildi ve işaretlendi: {OrderId}", pendingOrder.OrderId);

                // Değişiklikleri kaydet
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura gönderimi sırasında hata oluştu: {OrderId}", pendingOrder.OrderId);
            }

            return Ok(new { message = "Fatura gönderimi tamamlandı." });
        }

        [HttpGet("last-sent-invoices")]
        public async Task<IActionResult> GetLastSentInvoices()
        {
            var sentInvoices = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.AppUser)
                .Include(o => o.Vend)
                .Where(o => o.IsInvoiceSent)
                .OrderByDescending(o => o.OrderDate)
                .Take(20)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.TotalPrice,
                    o.Quantity,
                    ProductName = o.Product.Name,
                    UserName = o.AppUser.UserName,
                    Email = o.AppUser.Email,
                    VendName = o.Vend.Name
                })
                .ToListAsync();

            return Ok(sentInvoices);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<OrderListDto>>> GetAllOrders()
        {
            _logger.LogInformation("Tüm siparişler getiriliyor.");

            var orders = await _context.Orders
                    .Include(o => o.Product)
                    .Include(o => o.Vend)
                    .Include(o => o.AppUser) // kullanıcı dahil
                    .Select(o => new
                    {
                        o.OrderId,
                        o.Quantity,
                        o.TotalPrice,
                        o.OrderDate,
                        ProductName = o.Product.Name,
                        VendName = o.Vend.Name,
                        UserName = o.AppUser != null ? o.AppUser.FullName : "N/A",
                        UserEmail = o.AppUser != null ? o.AppUser.Email : "N/A"
                    })
                    .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<Order>> GetOrderDetail(int id)
        {
            _logger.LogInformation("Sipariş detayı getiriliyor: {OrderId}", id);

            var order = await _context.Orders
                .Include(o => o.Product)  // Ürün bilgilerini dahil et
                .Include(o => o.Vend)     // Satış noktasını dahil et
                .Include(o => o.AppUser)  // Kullanıcı bilgilerini dahil et
                .FirstOrDefaultAsync(o => o.OrderId == id);  // Belirli siparişi bul

            if (order == null)
            {
                _logger.LogWarning("Sipariş bulunamadı: {OrderId}", id);
                return NotFound(new { message = "Sipariş bulunamadı." });
            }

            // İlgili sipariş ve kullanıcı bilgilerini geri döndürüyoruz
            var orderDetail = new
            {
                order.OrderId,
                ProductName = order.Product.Name,
                order.Quantity,
                order.TotalPrice,
                VendName = order.Vend.Name,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                UserName = order.AppUser.FullName ?? order.AppUser.UserName, // Eğer FullName varsa onu al
                UserEmail = order.AppUser.Email
            };

            return Ok(orderDetail);  // JSON formatında geri döndür
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchOrders([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Arama kriteri boş olamaz.");
            }

            query = query.ToLower();

            var orders = await _context.Orders
                .Include(o => o.AppUser)
                .Include(o => o.Product)
                .Include(o => o.Vend)
                .Where(o => o.OrderId.ToString().Contains(query) ||
                            o.Product.Name.ToLower().Contains(query) ||
                            o.AppUser.FullName.ToLower().Contains(query) ||
                            o.Vend.Name.ToLower().Contains(query) ||
                            o.OrderDate.ToString().Contains(query))
                .Select(o => new
                {
                    o.OrderId,
                    ProductName = o.Product.Name,
                    UserName = o.AppUser.FullName,
                    VendName = o.Vend.Name,
                    o.OrderDate,
                    o.TotalPrice
                })
                .ToListAsync();

            return Ok(orders);
        }


    }
}