using Data.Repository.Order;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Service.Order
{
    public class OrderCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30); // Cứ 30 phút quét 1 lần

        public OrderCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    // Vì OrderRepository là Scoped, nên phải khởi tạo qua Scope trong BackgroundService
                    var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                    try
                    {
                        await orderRepository.CancelExpiredOrders();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi khi quét đơn hàng quá hạn: " + ex.Message);
                    }
                }

                // Đợi 30 phút rồi mới quét tiếp
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
