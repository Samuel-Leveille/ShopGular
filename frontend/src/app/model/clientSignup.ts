import { ClientSignupInterface } from "./clientSignupInterface";

export class ClientSignup implements ClientSignupInterface {
    firstname: string = "";
    name: string = "";
    email: string = "";
    password: string = "";
    confirmPassword: string = "";
}