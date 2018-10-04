import { Command, flags } from '@oclif/command'
import * as fs from 'fs'

import { FileNameBuilders } from '../fileNameBuilders'
import { TimeEntryModel, TimeEntryModel_A } from '../model/timeEntryModel'
import { TimeEntryFileService } from '../timeEntryFileService';

export default class Record extends Command {

  static username = "harry-plunger"
    
  static description = 'Record hours for a project. Defaults to 8 hours for today. '

  static examples = [
    `$ itt record <project> <hours> <YYYY-MM-DD>`,
  ]

  static flags = {
    help: flags.help(),
    date: flags.string({char: 'd', description: 'Override the date, format YYYY-MM-DD or MM-DD or DD', default: "today"})
  }

  static args = [
    {name: 'project', description: 'Project to record hours for', required: true},
    {name: 'hours', description: 'Hours to record, 7.5 is 7h and 30m', default: '8'},
  ]

  async run(): Promise<any> {
    const {args, flags} = this.parse(Record)
    const now = new Date();
    // todo: validate date is coming in correctlty, accept today or yesterday, or above examples

    const myDate = new Date(flags.date)
    console.log("the date is " + myDate)
    if( myDate == undefined ) {
      console.log("DATE is undefined! exiting")
    }
    if( args.project == undefined ) {
      console.log("default project is undefined! exiting")
      return
    }

    const entry: TimeEntryModel_A = {
      project: args.project,
      hours: args.hours,
      username: Record.username,
      date: flags.date == 'today' ? `${now.getFullYear()}-${now.getMonth() + 1}-${now.getDate()}` : `${flags.date.trim()}`,
      logDate: flags.date == 'today' ? 'today' : `on ${flags.date.trim()}`
    }
    let fileSvc = new TimeEntryFileService();
    fileSvc.addTimeEntry(entry, Record.username);

    this.log(`${entry.username} logged ${entry.hours} hours ${entry.logDate} for ${entry.project}`)
  }

}
