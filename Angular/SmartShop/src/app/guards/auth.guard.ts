import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { isLoggedIn } from "../rxjs/auth.operator";


export const authGuard:CanActivateFn = () => {
    const router = inject(Router);

    const userStatus = isLoggedIn();
    console.log("Auth guard - user status:", userStatus);
    console.log("Auth guard - token in storage:", sessionStorage.getItem("token"));
    if (userStatus) {
        console.log("Auth guard - allowing access");
        return true;
    }
    console.log("Auth guard - redirecting to login");
    router.navigate(["login"]);
    return false;
}