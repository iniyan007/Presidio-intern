import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LoginModel } from '../models/login.model';
import { catchError, map, tap } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    constructor(private http: HttpClient) {
    }
    public login(loginModel: LoginModel) {
        const url = 'https://dummyjson.com/user/login';
        return this.http.post(url, loginModel).pipe(
            tap(() => console.log('AuthService: login request sent')),
            map((response) => response),
            catchError((error) => {
                console.error('AuthService: login failed', error);
                return throwError(() => error);
            })
        );
    }
}   