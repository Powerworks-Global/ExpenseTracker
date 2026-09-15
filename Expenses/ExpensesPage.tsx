import { useState } from 'react';
import { Button } from 'primereact/button';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Tag } from 'primereact/tag';
import { Guid, DateOnly } from '@cratis/fundamentals';
import { SubmitExpense } from './Approval/SubmitExpense';
import { ApproveExpense, AllExpenses, ExpenseStatus } from './Approval/ApproveExpense';
import { RejectExpense } from './Approval/RejectExpense';

const CATEGORIES = ['Travel', 'Meals', 'Software', 'Office Supplies', 'Equipment', 'Other'];

const STATUS_SEVERITY: Record<string, 'info' | 'success' | 'danger' | 'warning'> = {
    PendingApproval: 'warning',
    Approved: 'info',
    Rejected: 'danger',
    PayoutRequested: 'info',
    Paid: 'success',
};

export const ExpensesPage = () => {
    const [expensesResult] = AllExpenses.use();
    const expenses = expensesResult.data;
    const [amount, setAmount] = useState('');
    const [description, setDescription] = useState('');
    const [category, setCategory] = useState(CATEGORIES[0]);
    const [date, setDate] = useState(() => new Date().toISOString().slice(0, 10));
    const [isBusinessExpense, setIsBusinessExpense] = useState(true);
    const [receiptUrl, setReceiptUrl] = useState<string | undefined>(undefined);
    const [uploading, setUploading] = useState(false);
    const [submitError, setSubmitError] = useState<string | undefined>(undefined);

    const uploadReceipt = async (file: File) => {
        setUploading(true);
        setSubmitError(undefined);
        try {
            const formData = new FormData();
            formData.append('file', file);
            const response = await fetch('/api/receipts', { method: 'POST', body: formData });
            if (!response.ok) {
                setSubmitError(await response.text() || 'Receipt upload failed');
                return;
            }
            const { url } = await response.json();
            setReceiptUrl(url);
        } finally {
            setUploading(false);
        }
    };

    const submit = async () => {
        setSubmitError(undefined);
        const command = new SubmitExpense();
        command.amount = Number(amount);
        command.description = description;
        command.category = category;
        command.date = DateOnly.parse(date);
        command.isBusinessExpense = isBusinessExpense;
        command.receiptUrl = receiptUrl;

        const result = await command.execute();
        if (!result.isSuccess) {
            setSubmitError(result.exceptionMessages?.[0] ?? result.validationResults?.[0]?.message ?? 'Could not submit expense');
            return;
        }

        setAmount('');
        setDescription('');
        setCategory(CATEGORIES[0]);
        setDate(new Date().toISOString().slice(0, 10));
        setIsBusinessExpense(true);
        setReceiptUrl(undefined);
    };

    const approve = async (expenseId: Guid) => {
        const command = new ApproveExpense();
        command.expenseId = expenseId;
        command.approvedBy = 'you';
        command.reason = 'Manual';
        await command.execute();
    };

    const reject = async (expenseId: Guid) => {
        const command = new RejectExpense();
        command.expenseId = expenseId;
        command.rejectedBy = 'you';
        await command.execute();
    };

    return (
        <div className='p-4 max-w-4xl mx-auto'>
            <h1 className='text-2xl font-bold mb-4'>Expenses</h1>

            <div className='border rounded-lg p-4 mb-6 flex flex-col gap-3'>
                <h2 className='font-semibold'>Submit an expense</h2>
                <div className='flex flex-wrap gap-3'>
                    <input
                        type='number' step='0.01' placeholder='Amount'
                        value={amount} onChange={e => setAmount(e.target.value)}
                        className='border rounded px-2 py-1 w-32' />
                    <input
                        type='text' placeholder='Description'
                        value={description} onChange={e => setDescription(e.target.value)}
                        className='border rounded px-2 py-1 flex-1 min-w-40' />
                    <select
                        value={category} onChange={e => setCategory(e.target.value)}
                        className='border rounded px-2 py-1'>
                        {CATEGORIES.map(c => <option key={c} value={c}>{c}</option>)}
                    </select>
                    <input
                        type='date' value={date} onChange={e => setDate(e.target.value)}
                        className='border rounded px-2 py-1' />
                    <label className='flex items-center gap-1 text-sm'>
                        <input type='checkbox' checked={isBusinessExpense} onChange={e => setIsBusinessExpense(e.target.checked)} />
                        Business expense
                    </label>
                </div>
                <div className='flex items-center gap-3'>
                    <input
                        type='file' accept='image/jpeg,image/png,image/webp,image/heic'
                        onChange={e => e.target.files?.[0] && uploadReceipt(e.target.files[0])}
                        className='text-sm' />
                    {uploading && <span className='text-sm text-gray-500'>Uploading…</span>}
                    {receiptUrl && !uploading && <span className='text-sm text-green-600'>Receipt attached ✓</span>}
                </div>
                {submitError && <div className='text-sm text-red-600'>{submitError}</div>}
                <Button label='Submit expense' icon='pi pi-plus' onClick={submit} disabled={!amount || !description || uploading} className='self-start' />
            </div>

            <DataTable value={expenses} emptyMessage='No expenses submitted yet.'>
                <Column field='description' header='Description' />
                <Column field='category' header='Category' />
                <Column field='amount' header='Amount' body={(row: ExpenseStatus) => row.amount.toFixed(2)} />
                <Column field='date' header='Date' body={(row: ExpenseStatus) => row.date?.toString()} />
                <Column header='Status' body={(row: ExpenseStatus) => <Tag value={row.status} severity={STATUS_SEVERITY[row.status]} />} />
                <Column header='Receipt' body={(row: ExpenseStatus) => row.receiptUrl
                    ? <a href={row.receiptUrl} target='_blank' rel='noopener noreferrer'>View</a>
                    : '—'} />
                <Column header='Actions' body={(row: ExpenseStatus) => row.status === 'PendingApproval' ? (
                    <div className='flex gap-2'>
                        <Button label='Approve' size='small' severity='success' onClick={() => approve(row.id)} />
                        <Button label='Reject' size='small' severity='danger' onClick={() => reject(row.id)} />
                    </div>
                ) : null} />
            </DataTable>
        </div>
    );
};
