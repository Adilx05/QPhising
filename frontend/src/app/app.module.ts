import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ApiRuntimeService } from './core/api/api-runtime.service';
import { AuthSessionService } from './core/auth/auth-session.service';
import { UnauthorizedPageComponent } from './core/auth/unauthorized-page.component';
import { LayoutShellComponent } from './core/layout/layout-shell.component';

function initializeApplication(apiRuntimeService: ApiRuntimeService, authSessionService: AuthSessionService): () => void {
  return () => {
    apiRuntimeService.configure();
    authSessionService.bootstrapSessionFromTokenClaims();
  };
}

@NgModule({
  declarations: [AppComponent, LayoutShellComponent, UnauthorizedPageComponent],
  imports: [BrowserModule, BrowserAnimationsModule, RouterModule, AppRoutingModule],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApplication,
      deps: [ApiRuntimeService, AuthSessionService],
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
