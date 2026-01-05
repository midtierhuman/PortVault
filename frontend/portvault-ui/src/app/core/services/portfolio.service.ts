import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Holding } from '../../models/holding.model';
import { Portfolio } from '../../models/portfolio.model';
import { Transaction } from '../../models/transaction.model';

@Injectable({ providedIn: 'root' })
export class PortfolioService {
  #http = inject(HttpClient);
  #base = `${environment.apiUrl}/Portfolio`;

  getAll() {
    return firstValueFrom(this.#http.get<Portfolio[]>(this.#base));
  }

  getHoldings(portfolioName: string) {
    return firstValueFrom(this.#http.get<Holding[]>(`${this.#base}/${portfolioName}/getholdings`));
  }

  getTransactions(portfolioName: string) {
    return firstValueFrom(
      this.#http.get<Transaction[]>(`${this.#base}/${portfolioName}/transactions`)
    );
  }

  updateTransaction(portfolioName: string, transaction: Transaction) {
    return firstValueFrom(
      this.#http.put<Transaction>(
        `${this.#base}/${portfolioName}/transactions/${transaction.id}`,
        transaction
      )
    );
  }
}
