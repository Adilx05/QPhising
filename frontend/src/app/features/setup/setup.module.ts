import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SetupPageComponent } from './containers/setup-page.component';
import { SetupRoutingModule } from './setup-routing.module';

@NgModule({
  declarations: [SetupPageComponent],
  imports: [CommonModule, SetupRoutingModule]
})
export class SetupModule {}
