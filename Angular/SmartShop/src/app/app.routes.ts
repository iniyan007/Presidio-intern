import { Routes } from '@angular/router';
import { Login } from './component/login/login';
import { Dashboard } from './component/dashboard/dashboard';
import { authGuard } from './guards/auth.guard';
import { Products } from './component/products/products';
import { ProductDetails } from './component/product-details/product-details';
import { Profile } from './component/profile/profile';

export const routes: Routes = [
    {path:'login', component:Login},
    {path:'',component:Dashboard,
        canActivate:[authGuard],
        children:[
            {
                path:'products',component:Products,
            },
            {
                path: 'products/:id',component: ProductDetails
            },
            {
                path:'profile',component: Profile
            }
        ]
     },
];
