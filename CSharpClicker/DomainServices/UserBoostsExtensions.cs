using CSharpClicker.Domain;

namespace CSharpClicker.DomainServices;

public static class UserBoostsExtensions
{
    // 0.5 = каждые 20 зданий удваивают эффективность
    private const double QuantitySynergyFactor = 0.05;

    // сколько процентов от дохода в секунду добавляется к клику (5%)
    private const double PassiveToClickRatio = 0.05;

    public static long GetProfitPerClick(this ICollection<UserBoost> userBoosts)
    {
        // 1. базовая сила клика (всегда 1 минимум)
        long baseClick = 1;

        // 2. добавляем силу от "ручных" бустов (кирка, динамит)
        // формула: (база * кол-во) * (1 + кол-во * 0.05)
        // пример: 10 Динамитов (сила 100) дадут не 1000, а 1500 урона
        var manualBoostsProfit = userBoosts
            .Where(ub => !ub.Boost.IsAuto)
            .Sum(ub => ub.Boost.Profit * ub.Quantity * (1 + ub.Quantity * QuantitySynergyFactor));

        // 3. считаем текущий авто-доход (чтобы добавить процент от него к клику)
        var passiveIncome = userBoosts.GetProfitPerSecond();

        // итого: база + ручные бусты + 5% от авто-дохода
        return (long)(baseClick + manualBoostsProfit + (passiveIncome * PassiveToClickRatio));
    }

    public static long GetProfitPerSecond(this ICollection<UserBoost> userBoosts)
    {
        // считаем доход от авто-бустов с учетом синергии количества
        return (long)userBoosts
            .Where(ub => ub.Boost.IsAuto)
            .Sum(ub => ub.Boost.Profit * ub.Quantity * (1 + ub.Quantity * QuantitySynergyFactor));
    }
}


