import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiResponse } from '../models/api-response.model';
import { AstronautDuty, CreateAstronautDuty, AstronautDutiesResponse } from '../models/astronaut-duty.model';

@Injectable({
  providedIn: 'root'
})
export class AstronautDutyService {
  private apiUrl = '/api/AstronautDuty';

  constructor(private http: HttpClient) { }

  getAstronautDutiesByName(name: string): Observable<AstronautDutiesResponse> {
    const encodedName = encodeURIComponent(name);
    return this.http.get<ApiResponse<AstronautDutiesResponse>>(`${this.apiUrl}/${encodedName}`).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.message || `Astronaut duties for '${name}' not found`);
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  createAstronautDuty(duty: CreateAstronautDuty): Observable<AstronautDuty> {
    return this.http.post<ApiResponse<AstronautDuty>>(this.apiUrl, duty).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.message || 'Failed to create astronaut duty');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  private handleError(error: any): Observable<never> {
    let errorMessage = 'An unexpected error occurred';
    
    if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.message) {
      errorMessage = error.message;
    }

    console.error('AstronautDutyService error:', error);
    return throwError(() => new Error(errorMessage));
  }
}

