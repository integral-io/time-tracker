import {Command, flags} from '@oclif/command'
import { TimeEntryFileService } from '../timeEntryFileService';
import { TimeEntryModel } from '../model/timeEntryModel';

export default class Report extends Command {
    
  static description = 'Builds report for recorded summary for a month or current year.'

  static examples = [
    `$ itt report <YYYY-MM>`,
  ]
  static args = [
    {name: 'date', description: 'date', required: false}
  ]

  async run(): Promise<any> {

    const {args, flags} = this.parse(Report)

    if(!args.date) {
      let today = new Date();
      args.date = `${today.getFullYear()}-${(today.getMonth() + 1).toString().padStart(2, '0')}`
    } 

    let timeEntryFileService = new TimeEntryFileService()    

    let timeEntries: TimeEntryModel[] = await timeEntryFileService.readTimeEntryFileForUsername( 'harry-plunger' )

    timeEntries = timeEntries.filter(
      entry => entry.date.startsWith(args.date)
    )

    // let p = timeEntries
    //   .map(
    //     item => JSON.stringify(item)
    //   ).reduce( (a, b) => a + '\n' + b )

    let hours = timeEntries
        .map(item => item.hours)
        .reduce( (a, b) => a + b )

    // this.log(p)
    this.log(`Total hours worked for ${ args.date }: ${ hours }`)

    //this.error('failing!', {exit: 100})
    // throw new Error("Method not implemented.");
  }
}