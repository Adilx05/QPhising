import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedModule } from '../../shared/shared.module';
import { TasksRoutingModule } from './tasks-routing.module';
import { TasksPageComponent } from './containers/tasks-page.component';
import { TaskQueueTableComponent } from './components/task-queue-table.component';

@NgModule({
  declarations: [TasksPageComponent, TaskQueueTableComponent],
  imports: [CommonModule, SharedModule, TasksRoutingModule]
})
export class TasksModule {}
