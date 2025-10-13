import { UserSignupInterface } from "./userSignupInterface";

export class UserSignup implements UserSignupInterface {
    firstname: string = "";
    name: string = "";
    email: string = "";
    password: string = "";
    confirmPassword: string = "";
}