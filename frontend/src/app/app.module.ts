import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ApiRuntimeService } from './core/api/api-runtime.service';
import { AuthService } from './core/auth/auth.service';
import { UnauthorizedPageComponent } from './core/auth/unauthorized-page.component';
import { LayoutShellComponent } from './core/layout/layout-shell.component';

function initializeApplication(authService: AuthService, apiRuntimeService: ApiRuntimeService): () => Promise<void> {
  return async () => {
    apiRuntimeService.configure();
    await authService.initialize();
  };
}

@NgModule({
  declarations: [AppComponent, LayoutShellComponent, UnauthorizedPageComponent],
  imports: [BrowserModule, BrowserAnimationsModule, RouterModule, AppRoutingModule],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApplication,
      deps: [AuthService, ApiRuntimeService],
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
