import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedModule } from '../../shared/shared.module';
import { ExportsRoutingModule } from './exports-routing.module';
import { ExportsPageComponent } from './containers/exports-page.component';
import { ExportsTableComponent } from './components/exports-table.component';

@NgModule({
  declarations: [ExportsPageComponent, ExportsTableComponent],
  imports: [CommonModule, SharedModule, ExportsRoutingModule]
})
export class ExportsModule {}
