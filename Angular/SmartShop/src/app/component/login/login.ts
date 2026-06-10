import { Component, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { LoginModel } from '../../models/login.model';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { form, minLength, required } from '@angular/forms/signals';
import { changeUsername, setUserInfo } from '../../rxjs/auth.operator';

@Component({
  selector: 'app-login',
  imports: [FormsModule, ReactiveFormsModule, FormField],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  loginModel = signal(new LoginModel());
  progress = signal(false);
  constructor(private authService: AuthService, private router: Router) {
  }
  loginform = form(this.loginModel,(path)=>{
    required(path.username, {message:"Username is required"});
    minLength(path.username, 4, {message:"Username must be at least 4 characters long"});
    required(path.password, {message:"Password is required"});
   
  });
  handleLoginClick(){
    if(this.loginform().invalid()){
      alert("Please fix the errors in the form before submitting.");
      return;
    }

    this.progress.set(true);
    this.authService.login(this.loginModel()).subscribe({
      next: (response:any) => {
        console.log("Full login response:", JSON.stringify(response, null, 2));
        console.log("Response object keys:", Object.keys(response));
        const token = response.token || response.accessToken || response.access_token;
        
        if (token) {
          sessionStorage.setItem('token', token);
          console.log("Token saved to storage");
          setUserInfo({
            id: response.id,
            username: response.username,
            email: response.email,
            firstName: response.firstName,
            lastName: response.lastName,
            gender: response.gender,
            image: response.image,
            accessToken: response.accessToken || response.token || response.access_token,
            refreshToken: response.refreshToken,
          });
          
          this.progress.set(false);
          changeUsername();
          this.router.navigate(['/']).then(success => {
            console.log("Navigation result:", success);
            if(!success) {
              alert("Navigation failed");
            }
          });
        } else {
          console.error("No token found in response. Full response:", response);
          alert("Login failed: No token received. Check console for response details.");
          this.progress.set(false);
        }
      },
      error: (error) => {
        console.error("Login failed", error);
        alert("Login failed. Please try again.");
        this.progress.set(false);
      }
    });
    
  }
}

