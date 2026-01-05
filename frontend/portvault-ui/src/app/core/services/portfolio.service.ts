import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Holding } from '../../models/holding.model';
import { Portfolio } from '../../models/portfolio.model';
import { Transaction } from '../../models/transaction.model';

export interface TransactionPage {
  data: Transaction[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AnalyticsHistory {
  date: string;
  invested: number;
}

export interface SegmentAllocation {
  segment: string;
  value: number;
  percentage: number;
}

export interface PortfolioAnalytics {
  history: AnalyticsHistory[];
  segmentAllocation: SegmentAllocation[];
}

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

  getTransactions(
    portfolioName: string,
    opts: {
      page?: number;
      pageSize?: number;
      from?: string | null;
      to?: string | null;
      search?: string | null;
    } = {}
  ) {
    let params = new HttpParams();
    if (opts.page) params = params.set('page', opts.page);
    if (opts.pageSize) params = params.set('pageSize', opts.pageSize);
    if (opts.from) params = params.set('from', opts.from);
    if (opts.to) params = params.set('to', opts.to);
    if (opts.search) params = params.set('search', opts.search);

    return firstValueFrom(
      this.#http.get<TransactionPage>(`${this.#base}/${portfolioName}/transactions`, { params })
    ).then(
      (res: any) =>
        ({
          data: res.data ?? res.Data ?? [],
          page: res.page ?? res.Page ?? 1,
          pageSize: res.pageSize ?? res.PageSize ?? opts.pageSize ?? 20,
          totalCount: res.totalCount ?? res.TotalCount ?? 0,
          totalPages: res.totalPages ?? res.TotalPages ?? 0,
        } satisfies TransactionPage)
    );
  }

  async getAllTransactions(
    portfolioName: string,
    opts: {
      from?: string | null;
      to?: string | null;
      search?: string | null;
      pageSize?: number;
    } = {}
  ): Promise<Transaction[]> {
    const pageSize = opts.pageSize ?? 500;
    let page = 1;
    const all: Transaction[] = [];

    while (true) {
      const res = await this.getTransactions(portfolioName, {
        page,
        pageSize,
        from: opts.from ?? undefined,
        to: opts.to ?? undefined,
        search: opts.search ?? undefined,
      });

      all.push(...res.data);

      if (page >= res.totalPages || res.data.length === 0) break;
      page += 1;
    }

    return all;
  }

  updateTransaction(portfolioName: string, transaction: Transaction) {
    return firstValueFrom(
      this.#http.put<Transaction>(
        `${this.#base}/${portfolioName}/transactions/${transaction.id}`,
        transaction
      )
    );
  }

  getAnalytics(portfolioName: string, duration: string = 'ALL', frequency: string = 'Daily') {
    const params = new HttpParams().set('duration', duration).set('frequency', frequency);

    return firstValueFrom(
      this.#http.get<PortfolioAnalytics>(`${this.#base}/${portfolioName}/analytics`, { params })
    );
  }
}
