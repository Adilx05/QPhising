import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LayoutShellComponent } from './core/layout/layout-shell.component';

@NgModule({
  declarations: [AppComponent, LayoutShellComponent],
  imports: [BrowserModule, BrowserAnimationsModule, RouterModule, AppRoutingModule],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
