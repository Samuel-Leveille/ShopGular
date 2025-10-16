import { UserLoginInterface } from "./userLoginInterface";

export class UserLogin implements UserLoginInterface {
    email: string = "";
    password: string = "";

    constructor(email: string, password: string) {
        this.email = email;
        this.password = password;
    }
}