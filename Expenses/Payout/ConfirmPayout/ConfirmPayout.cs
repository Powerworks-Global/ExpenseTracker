namespace CratisApp.Expenses.Payout.ConfirmPayout;

using Cratis.Arc.Commands.ModelBound;
using Cratis.Chronicle.Events;
using Cratis.Chronicle.Keys;
using Cratis.Monads;
using CratisApp.Expenses.Approval.ApproveExpense;

[Command]
public record ConfirmPayout([Key] Guid ExpenseId, string TransactionReference)
{
    public Result<ExpensePaid, string> Handle(ExpenseStatus? current)
    {
        if (current is null || current.Status != "PayoutRequested")
        {
            return "Expense payout has not been requested";
        }

        return new ExpensePaid(TransactionReference);
    }
}

[EventType]
public record ExpensePaid(string TransactionReference);
