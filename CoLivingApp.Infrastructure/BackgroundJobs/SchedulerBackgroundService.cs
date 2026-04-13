using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoLivingApp.Infrastructure.BackgroundJobs;

public class SchedulerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SchedulerBackgroundService> _logger;

    public SchedulerBackgroundService(IServiceProvider serviceProvider, ILogger<SchedulerBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("⏳ Умный планировщик задач запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                
                // Смотрим на 2 дня вперед!
                var lookAheadDate = DateTime.UtcNow.AddDays(2); 

                var dueExpenses = await context.RecurringExpenses
                    .Where(e => e.IsActive && e.NextRunDate <= lookAheadDate)
                    .ToListAsync(stoppingToken);

                foreach (var rec in dueExpenses) await ProcessRecurringExpense(context, rec);

                var dueChores = await context.RecurringChores
                    .Where(c => c.IsActive && c.NextRunDate <= lookAheadDate)
                    .ToListAsync(stoppingToken);

                foreach (var rec in dueChores) await ProcessRecurringChore(context, rec);

                if (dueExpenses.Any() || dueChores.Any())
                {
                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation($"✅ Сгенерировано заранее: {dueExpenses.Count} платежей, {dueChores.Count} задач.");
                }
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Проверяем каждый час
        }
    }

    private async Task ProcessRecurringExpense(IApplicationDbContext context, RecurringExpense template)
    {
        var members = await context.ApartmentMembers.Where(m => m.ApartmentId == template.ApartmentId && m.IsActive).ToListAsync();
        if (members.Count > 1)
        {
            var expense = new Expense
            {
                ApartmentId = template.ApartmentId,
                PayerId = template.PayerId,
                Amount = template.Amount,
                Description = $"🔄 {template.Description}", // Помечаем как регулярный!
                Category = template.Category,
                Date = template.NextRunDate // Дата чека - это дата, когда он реально должен списаться
            };

            decimal splitAmount = template.Amount / members.Count;
            foreach (var member in members)
                expense.Splits.Add(new ExpenseSplit { UserId = member.UserId, Amount = splitAmount });

            context.Expenses.Add(expense);
        }
        template.NextRunDate = CalculateNextRun(template.NextRunDate, template.Pattern, template.Interval);
    }

    private Task ProcessRecurringChore(IApplicationDbContext context, RecurringChore template)
    {
        var chore = new Chore
        {
            ApartmentId = template.ApartmentId,
            Title = $"🔄 {template.Title}", // Помечаем как регулярную!
            AssignedUserId = template.AssignedUserId,
            Status = ChoreStatus.Pending,
            DueDate = template.NextRunDate // Дедлайн - реальная дата из шаблона
        };

        context.Chores.Add(chore);
        template.NextRunDate = CalculateNextRun(template.NextRunDate, template.Pattern, template.Interval);
        return Task.CompletedTask;
    }

    private DateTime CalculateNextRun(DateTime currentRun, RecurrencePattern pattern, int interval)
    {
        return pattern switch
        {
            RecurrencePattern.Daily => currentRun.AddDays(interval),
            RecurrencePattern.Weekly => currentRun.AddDays(7 * interval),
            RecurrencePattern.Monthly => currentRun.AddMonths(interval),
            _ => currentRun.AddMonths(1)
        };
    }
}