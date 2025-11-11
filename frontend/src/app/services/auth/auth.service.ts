import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private readonly accessKey = 'sg.accessToken';
  private readonly refreshKey = 'sg.refreshToken';
  private readonly userNameKey = 'sg.userName';
  private readonly firstNameKey = 'sg.firstName';
  private readonly googlePendingKey = 'sg.googlePendingToken';
  private readonly api = environment.apiBaseUrl;

  private currentUserSubject = new BehaviorSubject<any | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  getCurrentUserSnapshot(): any | null {
    return this.currentUserSubject.value;
  }

  saveTokens(accessToken: string, refreshToken: string) {
    localStorage.setItem(this.accessKey, accessToken);
    localStorage.setItem(this.refreshKey, refreshToken);
  }

  saveUserName(name: string) {
    localStorage.setItem(this.userNameKey, name || '');
  }

  saveFirstName(firstName: string | null | undefined) {
    if (firstName) {
      localStorage.setItem(this.firstNameKey, firstName);
    } else {
      localStorage.removeItem(this.firstNameKey);
    }
  }

  getFirstName(): string | null {
    return localStorage.getItem(this.firstNameKey);
  }

  setCurrentUserFromLoginResponse(res: any) {
    this.currentUserSubject.next({ type: res?.type, user: res?.user || null });
    if (res?.type === 'client') {
      this.saveFirstName(res.user?.FirstName || res.user?.firstName || res.user?.firstname);
      this.saveUserName(res.user?.Name || res.user?.name);
    } else if (res?.type === 'seller') {
      this.saveFirstName(null);
      this.saveUserName(res.user?.Name || res.user?.name);
    } else {
      this.saveFirstName(null);
      this.saveUserName(res.user?.Name || res.user?.name);
    }
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshKey);
  }

  clearTokens() {
    localStorage.removeItem(this.accessKey);
    localStorage.removeItem(this.refreshKey);
    localStorage.removeItem(this.userNameKey);
    localStorage.removeItem(this.firstNameKey);
    this.currentUserSubject.next(null);
  }

  refresh() {
    const rt = this.getRefreshToken();
    return this.http.post<{ accessToken: string, accessTokenExpiresAtUtc: string }>(this.api + 'refresh', { refreshToken: rt });
  }

  getGoogleClientId(): string {
    return environment.googleClientId;
  }

  setGooglePendingToken(token: string) {
    const storage = this.sessionStorageSafe();
    storage?.setItem(this.googlePendingKey, token);
  }

  peekGooglePendingToken(): string | null {
    const storage = this.sessionStorageSafe();
    return storage?.getItem(this.googlePendingKey) ?? null;
  }

  consumeGooglePendingToken(): string | null {
    const token = this.peekGooglePendingToken();
    if (token) {
      const storage = this.sessionStorageSafe();
      storage?.removeItem(this.googlePendingKey);
    }
    return token;
  }

  clearGooglePendingToken() {
    const storage = this.sessionStorageSafe();
    storage?.removeItem(this.googlePendingKey);
  }

  private sessionStorageSafe(): Storage | null {
    if (typeof window === 'undefined') {
      return null;
    }
    try {
      return window.sessionStorage;
    } catch {
      return null;
    }
  }
}


