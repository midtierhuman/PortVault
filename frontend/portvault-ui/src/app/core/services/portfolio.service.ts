import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Holding } from '../../models/holding.model';
import { Portfolio } from '../../models/portfolio.model';
import {
  Transaction,
  TransactionPage,
  CreateTransactionRequest,
  TransactionUploadResponse,
} from '../../models/transaction.model';
import { PortfolioAnalytics } from '../../models/analytics.model';
import { ApiResponse } from '../../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class PortfolioService {
  #http = inject(HttpClient);
  #base = `${environment.apiUrl}/Portfolio`;

  getAll() {
    return this.#http
      .get<ApiResponse<Portfolio[]>>(this.#base)
      .pipe(map((res: ApiResponse<Portfolio[]>) => res.data || []));
  }

  getHoldings(portfolioName: string) {
    return this.#http
      .get<ApiResponse<Holding[]>>(`${this.#base}/${portfolioName}/getholdings`)
      .pipe(map((res: ApiResponse<Holding[]>) => res.data || []));
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

    return this.#http
      .get<ApiResponse<TransactionPage>>(`${this.#base}/${portfolioName}/transactions`, {
        params,
      })
      .pipe(
        map((res: ApiResponse<TransactionPage>) => {
          const data = res.data;
          return {
            data: data?.data ?? [],
            page: data?.page ?? 1,
            pageSize: data?.pageSize ?? opts.pageSize ?? 20,
            totalCount: data?.totalCount ?? 0,
            totalPages: data?.totalPages ?? 0,
          } satisfies TransactionPage;
        })
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
      const res = await firstValueFrom(
        this.getTransactions(portfolioName, {
          page,
          pageSize,
          from: opts.from ?? undefined,
          to: opts.to ?? undefined,
          search: opts.search ?? undefined,
        })
      );

      all.push(...res.data);

      if (page >= res.totalPages || res.data.length === 0) break;
      page += 1;
    }

    return all;
  }

  updateTransaction(portfolioName: string, transaction: Transaction) {
    return this.#http
      .put<ApiResponse<Transaction>>(
        `${this.#base}/${portfolioName}/transactions/${transaction.id}`,
        transaction
      )
      .pipe(map((res: ApiResponse<Transaction>) => res.data!));
  }

  getAnalytics(portfolioName: string, duration: string = 'ALL', frequency: string = 'Daily') {
    const params = new HttpParams().set('duration', duration).set('frequency', frequency);

    return this.#http
      .get<ApiResponse<PortfolioAnalytics>>(`${this.#base}/${portfolioName}/analytics`, {
        params,
      })
      .pipe(map((res: ApiResponse<PortfolioAnalytics>) => res.data!));
  }

  recalculateHoldings(portfolioName: string) {
    return this.#http
      .put<ApiResponse<null>>(`${this.#base}/${portfolioName}/holdings/recalculate`, {})
      .pipe(map((res: ApiResponse<null>) => res));
  }

  addTransaction(portfolioName: string, request: CreateTransactionRequest) {
    return this.#http
      .post<ApiResponse<{ addedCount: number }>>(
        `${this.#base}/${portfolioName}/transactions`,
        request
      )
      .pipe(map((res: ApiResponse<{ addedCount: number }>) => res));
  }

  deleteTransaction(portfolioName: string, transactionId: string) {
    return this.#http
      .delete<ApiResponse<null>>(`${this.#base}/${portfolioName}/transactions/${transactionId}`)
      .pipe(map((res: ApiResponse<null>) => res));
  }

  clearAllTransactions(portfolioName: string) {
    return this.#http
      .delete<ApiResponse<null>>(`${this.#base}/${portfolioName}/transactions/all?confirm=true`)
      .pipe(map((res: ApiResponse<null>) => res));
  }

  uploadTransactions(portfolioName: string, file: File) {
    const formData = new FormData();
    formData.append('file', file);

    return this.#http
      .post<ApiResponse<TransactionUploadResponse>>(
        `${this.#base}/${portfolioName}/transactions/upload`,
        formData
      )
      .pipe(map((res: ApiResponse<TransactionUploadResponse>) => res));
  }

  downloadTemplate() {
    return this.#http.get(`${environment.apiUrl}/portfolio/transactions/template`, {
      responseType: 'blob',
    });
  }
}
