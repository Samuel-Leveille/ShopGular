import { SellerSignupInterface } from "./sellerSignupInterface";

export class SellerSignup implements SellerSignupInterface {
    name: string = "";
    email: string = "";
    password: string = "";
    confirmPassword: string = "";
}