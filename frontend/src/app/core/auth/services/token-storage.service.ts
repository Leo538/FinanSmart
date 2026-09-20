import { Injectable } from '@angular/core';

interface StoredTokens {
  accessToken: string;
  refreshToken: string;
}

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private readonly key = 'finansmart.auth.tokens';

  getAccessToken(): string | null {
    return this.read()?.accessToken ?? null;
  }

  getRefreshToken(): string | null {
    return this.read()?.refreshToken ?? null;
  }

  save(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.key, JSON.stringify({ accessToken, refreshToken } satisfies StoredTokens));
  }

  clear(): void {
    localStorage.removeItem(this.key);
  }

  private read(): StoredTokens | null {
    const raw = localStorage.getItem(this.key);
    if (!raw) return null;

    try {
      const value = JSON.parse(raw) as Partial<StoredTokens>;
      return typeof value.accessToken === 'string' && typeof value.refreshToken === 'string' ? value as StoredTokens : null;
    } catch {
      this.clear();
      return null;
    }
  }
}
