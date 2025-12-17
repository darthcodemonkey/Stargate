import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiResponse } from '../models/api-response.model';
import { Person, CreatePerson, UpdatePerson } from '../models/person.model';

@Injectable({
  providedIn: 'root'
})
export class PersonService {
  // Use relative URL - when served from UI.Server, this will be same-origin
  // The UI.Server should proxy to the API or CORS should be enabled on API
  private apiUrl = '/api/person';

  constructor(private http: HttpClient) { }

  getAllPeople(): Observable<Person[]> {
    return this.http.get<ApiResponse<Person[]>>(this.apiUrl).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.message || 'Failed to retrieve people');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  getPersonByName(name: string): Observable<Person> {
    const encodedName = encodeURIComponent(name);
    return this.http.get<ApiResponse<Person>>(`${this.apiUrl}/${encodedName}`).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.message || `Person '${name}' not found`);
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  createPerson(person: CreatePerson): Observable<Person> {
    return this.http.post<ApiResponse<Person>>(this.apiUrl, person).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.message || 'Failed to create person');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  updatePerson(name: string, person: UpdatePerson): Observable<Person> {
    const encodedName = encodeURIComponent(name);
    return this.http.put<ApiResponse<Person>>(`${this.apiUrl}/${encodedName}`, person).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.message || 'Failed to update person');
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

    console.error('PersonService error:', error);
    return throwError(() => new Error(errorMessage));
  }
}

