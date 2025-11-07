import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Holding } from '../../models/holding.model';

@Injectable({ providedIn: 'root' })
export class PortfolioService {
  #http = inject(HttpClient);
  #base = `${environment.apiUrl}/portfolio`;

  getAll() {
    return firstValueFrom(this.#http.get<Portfolio[]>(this.#base));
  }

  getHoldings(id: string) {
    let v = firstValueFrom(this.#http.get<Holding[]>(`${this.#base}/${id}/holdings`));
    return v;
  }
}
