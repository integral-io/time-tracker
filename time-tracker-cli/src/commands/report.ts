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

  static flags = {
    weekly: flags.boolean({
      char: 'w',
      description: 'Generate a weekly report',
    })
  }

  async run(): Promise<any> {

    const {args, flags} = this.parse(Report)

    if(!args.date) {
      let today = new Date();
      args.date = `${today.getFullYear()}-${(today.getMonth() + 1).toString().padStart(2, '0')}`
    } 

    let timeEntryFileService = new TimeEntryFileService()    

    let timeEntries: TimeEntryModel[] = 
        await timeEntryFileService.readTimeEntryFileForUsername( 'harry-plunger' )

    let reportString = ""
    if(flags.weekly){
      reportString = generateWeeklyReport(args.date,timeEntries)
    }
    else {
      reportString = generateTotalHoursReport(args.date, timeEntries)
    } 
    this.log(reportString)   
  }
}

function generateTotalHoursReport(date,timeEntries) {

  timeEntries = timeEntries.filter(
    entry => entry.date.startsWith(date)
  )

  let hours = 0
  if(0 == timeEntries.length) { 
  }
  else {
    hours = timeEntries
    .map(item => item.hours)
    .reduce( (a, b) => a + b )
  }
  return `Total hours worked for ${ date }: ${ hours }`
}

function generateWeeklyReport(date,timeEntries) {

  timeEntries = timeEntries.filter(
    entry => entry.date.startsWith(date)
  )

  let report = "hello report"
  if(0 == timeEntries.length) { 
  }
  else {
    report = timeEntries
      .map(item => item.date + ", " + item.project + ", " + item.hours + "\n")
      .reduce( (a, b) => a + b )
  }
  return report
}