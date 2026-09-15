namespace CratisApp.Expenses.Payout.RequestPayout;

using Cratis.Arc.Commands.ModelBound;
using Cratis.Chronicle.Events;
using Cratis.Chronicle.Keys;
using Cratis.Monads;
using CratisApp.Expenses.Approval.ApproveExpense;

[Command]
public record RequestPayout([Key] Guid ExpenseId)
{
    public Result<ExpensePayoutRequested, string> Handle(ExpenseStatus? current)
    {
        if (current is null || current.Status != "Approved")
        {
            return "Expense is not approved";
        }

        return new ExpensePayoutRequested();
    }
}

[EventType]
public record ExpensePayoutRequested;
