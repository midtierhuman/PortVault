import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PortfolioDetails } from '../../models/portfolio-details.model';

@Injectable({ providedIn: 'root' })
export class PortfolioService {
  #http = inject(HttpClient);
  #base = `${environment.apiUrl}/portfolio`;

  getAll() {
    return firstValueFrom(this.#http.get<Portfolio[]>(this.#base));
  }

  getOne(id: string) {
    let v = firstValueFrom(this.#http.get<PortfolioDetails>(`${this.#base}/${id}`));
    return v;
  }
}
