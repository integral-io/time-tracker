import {Command, flags} from '@oclif/command'

export default class Record extends Command {
    
  static description = 'Record hours for a project. Defaults to 8 hours for today. '

  static examples = [
    `$ itt record <project> <hours> <YYYY-MM-DD>`,
  ]
  
  async run(): Promise<any> {
    const {args, flags} = this.parse(Record)
    throw new Error("Method not implemented.");
  }
}