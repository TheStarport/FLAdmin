import { Outlet, createRootRoute } from '@tanstack/react-router'
import { Fragment } from 'react/jsx-runtime'

export const Route = createRootRoute({
  component: RootComponent,
})

function RootComponent() {
  return (
    <Fragment>
      <Outlet />
    </Fragment>
  )
}
