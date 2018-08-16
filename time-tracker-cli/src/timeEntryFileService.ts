import { TimeEntryModel } from './model/timeEntryModel';
import * as fs from 'fs';
import { Utilities } from './utilities';
import { FileNameBuilders } from './fileNameBuilders';


export class TimeEntryFileService {

  addTimeEntry(timeEntry: TimeEntryModel, username: string): void {
    let csv = this.stringifyTimeEntryToCsv(timeEntry);
    fs.appendFile(
      `${Utilities.getHomeDirectoryPath()}/${ username }-hours.json`,
      `${ csv }\n`,
      ( error ) => {
        if(error ) {
          console.error( error ) 
        }
      })
  }

  async readTimeEntryFile(username: string): Promise<TimeEntryModel[]> {
    let fileContents = "";

    await fs.readFile(FileNameBuilders.getTimeEntryHistoryFileName(username), 'utf8',
        (err, data) => {
          fileContents = data.toString();
        });

    return JSON.parse(fileContents) as TimeEntryModel[]
  }

  private stringifyTimeEntryToCsv(timeEntry: TimeEntryModel) {
    return `${timeEntry.date},${timeEntry.username},${timeEntry.project},${timeEntry.hours}`;
  }
}