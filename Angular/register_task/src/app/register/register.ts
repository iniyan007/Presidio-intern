import { Component, signal } from '@angular/core';
import { RegisterModel } from '../models/register.models';
import { FormsModule } from '@angular/forms';
import { RegisterApiService } from '../services/register.api.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  register = signal(new RegisterModel());
  constructor(private RegisterApiService: RegisterApiService) {
  }
  handleRegisterClick(){
    this.RegisterApiService.registerUser(this.register())
    .subscribe({
      next:(response) => {
        console.log(response);
      },
      error:(error) => {
        console.error(error);
      },
      complete:()=>{
        console.log("Request completed");
      }
    });
  }
}
