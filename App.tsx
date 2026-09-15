import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { Arc } from '@cratis/arc.react';
import { DialogComponents } from '@cratis/arc.react/dialogs';
import { BusyIndicatorDialog, ConfirmationDialog } from '@cratis/components/Dialogs';
import { ExpensesPage } from './Expenses/ExpensesPage';

function App() {
    return (
        <Arc>
            <DialogComponents confirmation={ConfirmationDialog} busyIndicator={BusyIndicatorDialog}>
                <BrowserRouter>
                    <Routes>
                        <Route path='/' element={<ExpensesPage />} />
                    </Routes>
                </BrowserRouter>
            </DialogComponents>
        </Arc>
    );
}

export default App;
