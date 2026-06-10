import { BehaviorSubject } from "rxjs";

export interface UserInfo {
    id?: number;
    username?: string;
    email?: string;
    firstName?: string;
    lastName?: string;
    gender?: string;
    image?: string;
    accessToken?: string;
    refreshToken?: string;
}

export const usernameSubject = new BehaviorSubject<string | undefined>(undefined);
export const userInfoSubject = new BehaviorSubject<UserInfo | undefined>(undefined);

export const logout = () => {
    sessionStorage.removeItem("token");
    sessionStorage.removeItem("userInfo");
    usernameSubject.next(undefined);
    userInfoSubject.next(undefined);
}

export const isLoggedIn = () => {
    const token = sessionStorage.getItem("token");
    return token?true:false;
}

export const setUserInfo = (userInfo: UserInfo) => {
    sessionStorage.setItem("userInfo", JSON.stringify(userInfo));
    const firstName = userInfo.firstName;
    const lastName = userInfo.lastName;
    if (firstName && lastName) {
        usernameSubject.next(`${firstName} ${lastName}`);
    } else if (userInfo.username) {
        usernameSubject.next(userInfo.username);
    }
    userInfoSubject.next(userInfo);
}

export const restoreAuthState = () => {
    const token = sessionStorage.getItem("token");
    const storedUserInfo = sessionStorage.getItem("userInfo");
    if (!token) {
        usernameSubject.next(undefined);
        userInfoSubject.next(undefined);
        return;
    }

    if (storedUserInfo) {
        try {
            const userInfo: UserInfo = JSON.parse(storedUserInfo);
            userInfoSubject.next(userInfo);
            if (userInfo.firstName && userInfo.lastName) {
                usernameSubject.next(`${userInfo.firstName} ${userInfo.lastName}`);
            } else if (userInfo.username) {
                usernameSubject.next(userInfo.username);
            }
            return;
        } catch (error) {
            console.error('Failed to parse stored userInfo', error);
        }
    }

    try {
        const payload = JSON.parse(atob(token.split(".")[1]));
        const firstName = payload["firstName"] || payload["given_name"] || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"];
        const lastName = payload["lastName"] || payload["family_name"] || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"];
        const username = payload["username"] || payload["name"] || (firstName && lastName ? `${firstName} ${lastName}` : firstName);

        if (username) {
            usernameSubject.next(username);
        }
        if (firstName || lastName) {
            const userInfo: UserInfo = {
                firstName: firstName || undefined,
                lastName: lastName || undefined,
                username: username || undefined,
            };
            userInfoSubject.next(userInfo);
            sessionStorage.setItem("userInfo", JSON.stringify(userInfo));
        }
    } catch (error) {
        console.error('Failed to restore auth state from token', error);
        usernameSubject.next(undefined);
        userInfoSubject.next(undefined);
    }
};

export const changeUsername = () => {
    restoreAuthState();
};