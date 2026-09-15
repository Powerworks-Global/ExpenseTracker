namespace CratisApp.Expenses.Payout;

using Cratis.Arc.Commands;
using Cratis.Chronicle.Events;
using Cratis.Chronicle.Reactors;
using CratisApp.Expenses.Approval.ApproveExpense;
using CratisApp.Expenses.Payout.RequestPayout;
// Aliased: this file's own namespace (CratisApp.Expenses.Payout) is the direct parent of both
// command namespaces below, so an unqualified `RequestPayout`/`ConfirmPayout` resolves to the
// nested namespace instead of the command type.
using RequestPayoutCommand = CratisApp.Expenses.Payout.RequestPayout.RequestPayout;
using ConfirmPayoutCommand = CratisApp.Expenses.Payout.ConfirmPayout.ConfirmPayout;

public class PayoutReactor(ICommandPipeline commands, IPaymentGateway gateway) : IReactor
{
    // Same discipline as AutoApprovalReactor: reactors issue commands, never write events directly,
    // so the PendingApproval/Approved/PayoutRequested guards stay in exactly one place each.
    [OnceOnly]
    public async Task Handle(ExpenseApproved @event, EventContext context)
    {
        var expenseId = Guid.Parse(context.EventSourceId.Value);
        await commands.Execute(new RequestPayoutCommand(expenseId));
    }

    [OnceOnly]
    public async Task Handle(ExpensePayoutRequested @event, EventContext context)
    {
        var expenseId = Guid.Parse(context.EventSourceId.Value);
        var transactionReference = await gateway.Pay(expenseId);
        await commands.Execute(new ConfirmPayoutCommand(expenseId, transactionReference));
    }
}
