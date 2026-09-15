namespace CratisApp.Expenses.Payout;

public interface IPaymentGateway
{
    // Amount is deliberately not passed: the mock gateway doesn't actually move money, and a real
    // gateway integration is out of v1 scope (see Expense Tracker.md's payout design) — the reactor
    // that calls this should stay signature-stable regardless of what a future real gateway needs.
    Task<string> Pay(Guid expenseId);
}

// Mock gateway per the design's explicit v1 scope: no real payment processor integration yet,
// just enough to prove the ExpensePayoutRequested -> ExpensePaid transition works end to end.
public class MockPaymentGateway : IPaymentGateway
{
    public Task<string> Pay(Guid expenseId) =>
        Task.FromResult($"mock-txn-{expenseId:N}");
}
