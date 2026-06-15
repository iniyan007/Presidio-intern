import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class RegisterApiService {
    constructor(private http : HttpClient) { }
    public registerUser(RegisterModel: any) {
        return this.http.post('https://localhost/5081/api/Auth/register', RegisterModel);
    }
}