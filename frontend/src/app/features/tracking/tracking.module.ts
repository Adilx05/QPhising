import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedModule } from '../../shared/shared.module';
import { TrackingRoutingModule } from './tracking-routing.module';
import { TrackingPageComponent } from './containers/tracking-page.component';
import { TrackingEventsTableComponent } from './components/tracking-events-table.component';

@NgModule({
  declarations: [TrackingPageComponent, TrackingEventsTableComponent],
  imports: [CommonModule, SharedModule, TrackingRoutingModule]
})
export class TrackingModule {}
