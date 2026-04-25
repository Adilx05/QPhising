import { bootstrapApplication } from '@angular/platform-browser';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import { AppComponent } from './app/app.component';
import { appRoutes } from './app/app.routes';
import { provideOpenApiConfiguration } from './app/core/config/openapi-config.provider';

bootstrapApplication(AppComponent, {
  providers: [provideRouter(appRoutes), provideAnimationsAsync(), provideOpenApiConfiguration(), providePrimeNG({ theme: { preset: Aura }, ripple: true })]
}).catch((error: unknown) => {
  console.error('QPhising frontend bootstrap failed.', error);
});
